using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Models;
using System.Text.Json;
using Utils;

namespace Services.Services
{
	public class AzureServiceBusReceiver : IReceiverService
	{
		private readonly ServiceBusClient _serviceBusClient;
		private readonly ServiceBusProcessor _serviceBusProcessor;

		public AzureServiceBusReceiver(IConfiguration configuration)
		{
			_serviceBusClient = new ServiceBusClient(configuration.GetConnectionString("AzureServiceBus"), new ServiceBusClientOptions
			{
				TransportType = ServiceBusTransportType.AmqpWebSockets
			});

			_serviceBusProcessor = _serviceBusClient.CreateProcessor(configuration.GetSection("ConnectionSettings")["QueueName"], new ServiceBusProcessorOptions());
		}

		public async Task StartJob()
		{
			_serviceBusProcessor.ProcessMessageAsync += MessageHandler;
			_serviceBusProcessor.ProcessErrorAsync += ErrorHandler;

			await _serviceBusProcessor.StartProcessingAsync();
		}

		public async Task FinishJob()
		{
			await _serviceBusProcessor.StopProcessingAsync();

			await _serviceBusProcessor.DisposeAsync();
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

		private async Task ErrorHandler(ProcessErrorEventArgs args)
		{
			await Task.Run(() =>
			{
				ConsoleUtils.WriteLineColor($"Exception occured: {args.Exception}", ConsoleColor.Red);
			});
		}
	}
}
