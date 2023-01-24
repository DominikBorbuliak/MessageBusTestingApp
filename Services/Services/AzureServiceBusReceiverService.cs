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

		private readonly ServiceBusProcessor _sendOnlyServiceBusProcessor;

		private readonly ServiceBusSessionProcessor _sendAndReplyServiceBusProcessor;
		private readonly ServiceBusSender _sendAndReplyServiceBusSender;

		public AzureServiceBusReceiverService(IConfiguration configuration)
		{
			_serviceBusClient = new ServiceBusClient(configuration.GetConnectionString("AzureServiceBus"), new ServiceBusClientOptions
			{
				TransportType = ServiceBusTransportType.AmqpWebSockets
			});

			_sendOnlyServiceBusProcessor = _serviceBusClient.CreateProcessor(configuration.GetSection("ConnectionSettings")["SendOnlyReceiverQueueName"], new ServiceBusProcessorOptions());
			_sendAndReplyServiceBusProcessor = _serviceBusClient.CreateSessionProcessor(configuration.GetSection("ConnectionSettings")["SendAndReplyReceiverQueueName"], new ServiceBusSessionProcessorOptions());

			_sendAndReplyServiceBusSender = _serviceBusClient.CreateSender(configuration.GetSection("ConnectionSettings")["SendAndReplySenderQueueName"]);
		}

		public async Task StartJob()
		{
			_sendOnlyServiceBusProcessor.ProcessMessageAsync += MessageHandler;
			_sendOnlyServiceBusProcessor.ProcessErrorAsync += ErrorHandler;

			await _sendOnlyServiceBusProcessor.StartProcessingAsync();

			_sendAndReplyServiceBusProcessor.ProcessMessageAsync += RectangularPrismRequestHandler;
			_sendAndReplyServiceBusProcessor.ProcessErrorAsync += ErrorHandler;

			await _sendAndReplyServiceBusProcessor.StartProcessingAsync();
		}

		public async Task FinishJob()
		{
			await _sendOnlyServiceBusProcessor.StopProcessingAsync();
			await _sendAndReplyServiceBusProcessor.StopProcessingAsync();

			await _sendOnlyServiceBusProcessor.DisposeAsync();
			await _sendAndReplyServiceBusProcessor.DisposeAsync();

			await _sendAndReplyServiceBusSender.DisposeAsync();

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

		private async Task RectangularPrismRequestHandler(ProcessSessionMessageEventArgs arguments)
		{
			var body = arguments.Message.Body.ToString();

			var rectangularPrismRequest = JsonSerializer.Deserialize<RectangularPrismRequest>(body);
			ConsoleUtils.WriteLineColor($"Rectangular prism request received:\n{rectangularPrismRequest}", ConsoleColor.Green);

			var rectangularPrismResponse = new RectangularPrismResponse
			{
				SurfaceArea = 2 * (rectangularPrismRequest.EdgeA * rectangularPrismRequest.EdgeB + rectangularPrismRequest.EdgeA * rectangularPrismRequest.EdgeC + rectangularPrismRequest.EdgeB * rectangularPrismRequest.EdgeC),
				Volume = rectangularPrismRequest.EdgeA * rectangularPrismRequest.EdgeB * rectangularPrismRequest.EdgeC
			};

			ConsoleUtils.WriteLineColor("Sending rectangular prism response", ConsoleColor.Green);

			var response = rectangularPrismResponse.ToServiceBusMessage();
			response.SessionId = arguments.SessionId;

			await _sendAndReplyServiceBusSender.SendMessageAsync(response);

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
