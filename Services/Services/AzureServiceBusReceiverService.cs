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

			_sendAndReplyServiceBusProcessor.ProcessMessageAsync += RequestHandler;
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

		/// <summary>
		/// Message handler used to process simple and advanced message
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		private async Task MessageHandler(ProcessMessageEventArgs arguments)
		{
			var body = arguments.Message.Body.ToString();

			if (arguments.Message.Subject.Equals(MessageType.SimpleMessage.GetDescription()))
			{
				ConsoleUtils.WriteLineColor($"Simple messsage received: {body}", ConsoleColor.Green);
			}
			else if (arguments.Message.Subject.Equals(MessageType.AdvancedMessage.GetDescription()))
			{
				var advancedMessage = JsonSerializer.Deserialize<AdvancedMessage>(body);

				if (!AdvancedMessageHandler.Handle(advancedMessage))
					return;
			}
			else if (arguments.Message.Subject.Equals(MessageType.ExceptionMessage.GetDescription()))
			{
				var exceptionMessage = JsonSerializer.Deserialize<ExceptionMessage>(body);

				if (!ExceptionMessageHandler.Handle(exceptionMessage, arguments.Message.DeliveryCount))
					return;
			}

			await arguments.CompleteMessageAsync(arguments.Message);
		}

		/// <summary>
		/// Request handler used to process rectangular prism request and process timeout request
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		private async Task RequestHandler(ProcessSessionMessageEventArgs arguments)
		{
			var body = arguments.Message.Body.ToString();

			if (arguments.Message.Subject.Equals(MessageType.RectangularPrismRequest.GetDescription()))
			{
				var rectangularPrismRequest = JsonSerializer.Deserialize<RectangularPrismRequest>(body);

				var rectangularPrismResponse = RectangularPrismRequestHandler.HandleAndGenerateResponse(rectangularPrismRequest, arguments.Message.DeliveryCount);
				if (rectangularPrismResponse == null)
					return;

				var response = rectangularPrismResponse.ToServiceBusMessage();
				response.SessionId = arguments.SessionId;

				await _sendAndReplyServiceBusSender.SendMessageAsync(response);
			}
			else if (arguments.Message.Subject.Equals(MessageType.ProcessTimeoutRequest.GetDescription()))
			{
				var processTimeoutRequest = JsonSerializer.Deserialize<ProcessTimeoutRequest>(body);

				var processTimeoutResponse = await ProcessTimeoutRequestHandler.HandleAndGenerateResponse(processTimeoutRequest);
				if (processTimeoutResponse == null)
					return;

				var response = processTimeoutResponse.ToServiceBusMessage();
				response.SessionId = arguments.SessionId;

				await _sendAndReplyServiceBusSender.SendMessageAsync(response);
			}

			await arguments.CompleteMessageAsync(arguments.Message);
		}

		/// <summary>
		/// Error handler which is trigerred when exception is thrown
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		private async Task ErrorHandler(ProcessErrorEventArgs args)
		{
			await Task.Run(() =>
			{
				ConsoleUtils.WriteLineColor($"Exception occured: {args.Exception.Message}", ConsoleColor.Red);
			});
		}
	}
}
