using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Handlers;
using Services.Mappers;
using Services.Models;
using System.Text.Json;
using Utils;

namespace Services.Services
{
	public class AzureServiceBusReceiverService : IReceiverService
	{
		private readonly ServiceBusClient _serviceBusClient;

		private readonly ServiceBusProcessor _sendOnlyServiceBusProcessor;

		private readonly ServiceBusSessionProcessor _sendAndReplyWaitServiceBusProcessor;
		private readonly ServiceBusSender _sendAndReplyWaitServiceBusSender;

		private readonly ServiceBusSessionProcessor _sendAndReplyNoWaitServiceBusProcessor;
		private readonly ServiceBusSender _sendAndReplyNoWaitServiceBusSender;

		/// <summary>
		/// Used to simulate simple error handling for no wait pattern
		/// </summary>
		private readonly int _maxRetries = 10;

		public AzureServiceBusReceiverService(IConfiguration configuration)
		{
			_serviceBusClient = new ServiceBusClient(configuration.GetConnectionString("AzureServiceBus"), new ServiceBusClientOptions
			{
				TransportType = ServiceBusTransportType.AmqpWebSockets,
				RetryOptions = new ServiceBusRetryOptions
				{
					MaxRetries = _maxRetries
				}
			});

			_sendOnlyServiceBusProcessor = _serviceBusClient.CreateProcessor(configuration.GetSection("ConnectionSettings")["SendOnlyReceiverQueueName"], new ServiceBusProcessorOptions());

			_sendAndReplyWaitServiceBusProcessor = _serviceBusClient.CreateSessionProcessor(configuration.GetSection("ConnectionSettings")["SendAndReplyWaitReceiverQueueName"], new ServiceBusSessionProcessorOptions());
			_sendAndReplyWaitServiceBusSender = _serviceBusClient.CreateSender(configuration.GetSection("ConnectionSettings")["SendAndReplyWaitSenderQueueName"]);

			_sendAndReplyNoWaitServiceBusSender = _serviceBusClient.CreateSender(configuration.GetSection("ConnectionSettings")["SendAndReplyNoWaitSenderQueueName"]);
			_sendAndReplyNoWaitServiceBusProcessor = _serviceBusClient.CreateSessionProcessor(configuration.GetSection("ConnectionSettings")["SendAndReplyNoWaitReceiverQueueName"]);
		}

		public async Task StartJob()
		{
			_sendOnlyServiceBusProcessor.ProcessMessageAsync += MessageHandler;
			_sendOnlyServiceBusProcessor.ProcessErrorAsync += ErrorHandler;

			await _sendOnlyServiceBusProcessor.StartProcessingAsync();

			_sendAndReplyWaitServiceBusProcessor.ProcessMessageAsync += RequestHandler;
			_sendAndReplyWaitServiceBusProcessor.ProcessErrorAsync += ErrorHandler;

			await _sendAndReplyWaitServiceBusProcessor.StartProcessingAsync();

			_sendAndReplyNoWaitServiceBusProcessor.ProcessMessageAsync += RequestHandler;
			_sendAndReplyNoWaitServiceBusProcessor.ProcessErrorAsync += ErrorHandler;

			await _sendAndReplyNoWaitServiceBusProcessor.StartProcessingAsync();
		}

		public async Task FinishJob()
		{
			await _sendOnlyServiceBusProcessor.StopProcessingAsync();
			await _sendAndReplyWaitServiceBusProcessor.StopProcessingAsync();
			await _sendAndReplyNoWaitServiceBusProcessor.StopProcessingAsync();

			await _sendOnlyServiceBusProcessor.DisposeAsync();
			await _sendAndReplyWaitServiceBusProcessor.DisposeAsync();
			await _sendAndReplyNoWaitServiceBusProcessor.DisposeAsync();

			await _sendAndReplyWaitServiceBusSender.DisposeAsync();
			await _sendAndReplyNoWaitServiceBusSender.DisposeAsync();

			await _serviceBusClient.DisposeAsync();
		}

		/// <summary>
		/// Message handler used to process simple, advanced message and exception
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		private async Task MessageHandler(ProcessMessageEventArgs arguments)
		{
			var body = arguments.Message.Body.ToString();

			var deserializedCorrectly = false;

			if (arguments.Message.Subject.Equals(MessageType.SimpleMessage.GetDescription()))
			{
				ConsoleUtils.WriteLineColor($"Simple messsage received: {body}", ConsoleColor.Green);
				deserializedCorrectly = true;
			}
			else if (arguments.Message.Subject.Equals(MessageType.AdvancedMessage.GetDescription()))
			{
				var advancedMessage = JsonSerializer.Deserialize<AdvancedMessage>(body);
				deserializedCorrectly = AdvancedMessageHandler.Handle(advancedMessage);
			}
			else if (arguments.Message.Subject.Equals(MessageType.ExceptionMessage.GetDescription()))
			{
				var exceptionMessage = JsonSerializer.Deserialize<ExceptionMessage>(body);
				deserializedCorrectly = ExceptionMessageHandler.Handle(exceptionMessage, arguments.Message.DeliveryCount);
			}

			// Complete message only if deserialization succeeded
			if (deserializedCorrectly)
				await arguments.CompleteMessageAsync(arguments.Message);
		}

		/// <summary>
		/// Request handler used to process rectangular prism request and process timeout request
		/// Used for Wait and No Wait
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		private async Task RequestHandler(ProcessSessionMessageEventArgs arguments)
		{
			var body = arguments.Message.Body.ToString();

			if (arguments.Message.Subject.Equals(MessageType.RectangularPrismWaitRequest.GetDescription()) || arguments.Message.Subject.Equals(MessageType.RectangularPrismNoWaitRequest.GetDescription()))
			{
				var rectangularPrismRequest = JsonSerializer.Deserialize<RectangularPrismRequest>(body);

				RectangularPrismResponse? rectangularPrismResponse;

				try
				{
					rectangularPrismResponse = RectangularPrismRequestHandler.HandleAndGenerateResponse(rectangularPrismRequest, arguments.Message.DeliveryCount);
				}
				catch
				{
					// Simulate simple error handling in No Wait
					if (arguments.Message.Subject.Equals(MessageType.RectangularPrismNoWaitRequest.GetDescription()) && _maxRetries == arguments.Message.DeliveryCount)
						await _sendAndReplyNoWaitServiceBusSender.SendMessageAsync(new ExceptionResponse { Text = "No response found for: RectangularPrismResponse!" }.ToServiceBusMessage(arguments.SessionId));

					// Error must be thrown in all cases to trigger error handler
					throw;
				}

				// Do not complete message if deserialization was not correct
				if (rectangularPrismResponse == null)
					return;

				var response = rectangularPrismResponse.ToServiceBusMessage(arguments.SessionId);

				// Use different sender for Wait and No Wait
				if (arguments.Message.Subject.Equals(MessageType.RectangularPrismWaitRequest.GetDescription()))
					await _sendAndReplyWaitServiceBusSender.SendMessageAsync(response);
				else
					await _sendAndReplyNoWaitServiceBusSender.SendMessageAsync(response);
			}
			else if (arguments.Message.Subject.Equals(MessageType.ProcessTimeoutWaitRequest.GetDescription()) || arguments.Message.Subject.Equals(MessageType.ProcessTimeoutNoWaitRequest.GetDescription()))
			{
				var processTimeoutRequest = JsonSerializer.Deserialize<ProcessTimeoutRequest>(body);

				var processTimeoutResponse = await ProcessTimeoutRequestHandler.HandleAndGenerateResponse(processTimeoutRequest);

				// Do not complete message if deserialization was not correct
				if (processTimeoutResponse == null)
					return;

				var response = processTimeoutResponse.ToServiceBusMessage(arguments.SessionId);

				// Use different sender for Wait and No Wait
				if (arguments.Message.Subject.Equals(MessageType.ProcessTimeoutWaitRequest.GetDescription()))
					await _sendAndReplyWaitServiceBusSender.SendMessageAsync(response);
				else
					await _sendAndReplyNoWaitServiceBusSender.SendMessageAsync(response);
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
