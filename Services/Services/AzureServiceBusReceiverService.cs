using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Models;
using System.Text.Json;
using Utils;

namespace Services.Services
{
	public class AzureServiceBusReceiverService : IReceiverService
	{
		private readonly ServiceBusClient _serviceBusClient;
		private readonly ServiceBusProcessor _serviceBusProcessor;
		private readonly ServiceBusSessionProcessor _serviceBusSessionProcessor;
		private readonly ServiceBusSender _serviceBusSessionSender;

		public AzureServiceBusReceiverService(IConfiguration configuration)
		{
			_serviceBusClient = new ServiceBusClient(configuration.GetConnectionString("AzureServiceBus"), new ServiceBusClientOptions
			{
				TransportType = ServiceBusTransportType.AmqpWebSockets
			});

			_serviceBusProcessor = _serviceBusClient.CreateProcessor(configuration.GetSection("ConnectionSettings")["ReceiverQueueName"], new ServiceBusProcessorOptions());
			_serviceBusSessionProcessor = _serviceBusClient.CreateSessionProcessor(configuration.GetSection("ConnectionSettings")["SessionReceiverQueueName"], new ServiceBusSessionProcessorOptions());

			_serviceBusSessionSender = _serviceBusClient.CreateSender(configuration.GetSection("ConnectionSettings")["SessionSenderQueueName"]);
		}

		public async Task StartJob()
		{
			_serviceBusProcessor.ProcessMessageAsync += MessageHandler;
			_serviceBusProcessor.ProcessErrorAsync += ErrorHandler;

			await _serviceBusProcessor.StartProcessingAsync();

			_serviceBusSessionProcessor.ProcessMessageAsync += SessionMessageHandler;
			_serviceBusSessionProcessor.ProcessErrorAsync += ErrorHandler;

			await _serviceBusSessionProcessor.StartProcessingAsync();
		}

		public async Task FinishJob()
		{
			await _serviceBusProcessor.StopProcessingAsync();
			await _serviceBusSessionProcessor.StopProcessingAsync();

			await _serviceBusProcessor.DisposeAsync();
			await _serviceBusSessionProcessor.DisposeAsync();

			await _serviceBusSessionSender.DisposeAsync();

			await _serviceBusClient.DisposeAsync();
		}

		private async Task MessageHandler(ProcessMessageEventArgs arguments)
		{
			var body = arguments.Message.Body.ToString();

			try
			{
				var advancedMessage = JsonSerializer.Deserialize<AdvancedMessage>(body);
				ConsoleUtils.WriteLineColor($"Advanced messsage received:\n{advancedMessage}", ConsoleColor.Green);
			}
			catch
			{
				ConsoleUtils.WriteLineColor($"Simple messsage received: {body}", ConsoleColor.Green);
			}

			await arguments.CompleteMessageAsync(arguments.Message);
		}

		private async Task SessionMessageHandler(ProcessSessionMessageEventArgs arguments)
		{
			var body = arguments.Message.Body.ToString();

			try
			{
				var advancedMessage = JsonSerializer.Deserialize<AdvancedMessage>(body);
				ConsoleUtils.WriteLineColor($"Advanced messsage received:\n{advancedMessage}", ConsoleColor.Green);
			}
			catch
			{
				ConsoleUtils.WriteLineColor($"Simple messsage received: {body}", ConsoleColor.Green);
			}

			ConsoleUtils.WriteLineColor("Sending response", ConsoleColor.Green);
			await _serviceBusSessionSender.SendMessageAsync(new ServiceBusMessage("Message was successfuly received") { SessionId = arguments.SessionId });

			await arguments.CompleteMessageAsync(arguments.Message);
		}

		private async Task ErrorHandler(ProcessErrorEventArgs args)
		{
			await Task.Run(() =>
			{
				ConsoleUtils.WriteLineColor($"Exception occured: {args.Exception}", ConsoleColor.Red);
			});
		}
	}
}
