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
	public class AzureServiceBusSenderService : ISenderService
	{
		private readonly IConfiguration _configuration;
		private readonly ServiceBusClient _serviceBusClient;

		private readonly ServiceBusSender _sendOnlyServiceBusSender;

		private readonly ServiceBusSender _sendAndReplyWaitServiceBusSender;

		private readonly ServiceBusSender _sendAndReplyNoWaitServiceBusSender;
		private readonly ServiceBusSessionProcessor _sendAndReplyNoWaitServiceBusProcessor;

		public AzureServiceBusSenderService(IConfiguration configuration)
		{
			_configuration = configuration;
			_serviceBusClient = new ServiceBusClient(_configuration.GetConnectionString("AzureServiceBus"), new ServiceBusClientOptions()
			{
				TransportType = ServiceBusTransportType.AmqpWebSockets
			});

			_sendOnlyServiceBusSender = _serviceBusClient.CreateSender(_configuration.GetSection("ConnectionSettings")["SendOnlyReceiverQueueName"]);

			_sendAndReplyWaitServiceBusSender = _serviceBusClient.CreateSender(_configuration.GetSection("ConnectionSettings")["SendAndReplyWaitReceiverQueueName"]);

			_sendAndReplyNoWaitServiceBusSender = _serviceBusClient.CreateSender(_configuration.GetSection("ConnectionSettings")["SendAndReplyNoWaitReceiverQueueName"]);
			_sendAndReplyNoWaitServiceBusProcessor = _serviceBusClient.CreateSessionProcessor(_configuration.GetSection("ConnectionSettings")["SendAndReplyNoWaitSenderQueueName"]);
			_sendAndReplyNoWaitServiceBusProcessor.ProcessMessageAsync += ResponseHandler;
			_sendAndReplyNoWaitServiceBusProcessor.ProcessErrorAsync += ErrorHandler;
			_sendAndReplyNoWaitServiceBusProcessor.StartProcessingAsync().Wait();
		}

		public async Task SendSimpleMessage(SimpleMessage simpleMessage) => await _sendOnlyServiceBusSender.SendMessageAsync(simpleMessage.ToServiceBusMessage());

		public async Task SendAdvancedMessage(AdvancedMessage advancedMessage) => await _sendOnlyServiceBusSender.SendMessageAsync(advancedMessage.ToServiceBusMessage());

		public async Task SendExceptionMessage(ExceptionMessage exceptionMessage) => await _sendOnlyServiceBusSender.SendMessageAsync(exceptionMessage.ToServiceBusMessage());

		public async Task SendAndReplyRectangularPrism(RectangularPrismRequest rectangularPrismRequest, bool wait)
		{
			// Each request must have it's own session id to allow concurent processing
			var sessionId = Guid.NewGuid().ToString();
			var serviceBusMessage = rectangularPrismRequest.ToServiceBusMessage(sessionId, wait);

			// Use different sender for Wait and No Wait
			if (!wait)
			{
				await _sendAndReplyNoWaitServiceBusSender.SendMessageAsync(serviceBusMessage);
				return;
			}

			// Each request should have it's own receiver
			var serviceBusReceiver = await _serviceBusClient.AcceptSessionAsync(_configuration.GetSection("ConnectionSettings")["SendAndReplyWaitSenderQueueName"], sessionId);

			await _sendAndReplyWaitServiceBusSender.SendMessageAsync(serviceBusMessage);

			var responseMessage = await serviceBusReceiver.ReceiveMessageAsync();

			// If null is returned by receiver it means that request was not processed due to repeating exception or timeout
			if (responseMessage == null)
			{
				ConsoleUtils.WriteLineColor("No response found for: RectangularPrismResponse!", ConsoleColor.Red);
				return;
			}

			var rectangularPrismResponse = JsonSerializer.Deserialize<RectangularPrismResponse>(responseMessage.Body);

			RectangularPrismResponseHandler.Handle(rectangularPrismResponse);

			await serviceBusReceiver.DisposeAsync();
		}

		public async Task SendAndReplyProcessTimeout(ProcessTimeoutRequest processTimeoutRequest, bool wait)
		{
			// Each request must have it's own session id to allow concurent processing
			var processSesionId = Guid.NewGuid().ToString();
			var serviceBusMessage = processTimeoutRequest.ToServiceBusMessage(processSesionId, wait);

			// Use different sender for Wait and No Wait
			if (!wait)
			{
				await _sendAndReplyNoWaitServiceBusSender.SendMessageAsync(serviceBusMessage);
				return;
			}

			// Each request must have it's own receiver to allow concurent processing
			var processServiceBusReceiver = await _serviceBusClient.AcceptSessionAsync(_configuration.GetSection("ConnectionSettings")["SendAndReplyWaitSenderQueueName"], processSesionId);

			await _sendAndReplyWaitServiceBusSender.SendMessageAsync(serviceBusMessage);

			var responseMessage = await processServiceBusReceiver.ReceiveMessageAsync();

			// If null is returned by receiver it means that request was not processed due to repeating exception or timeout
			if (responseMessage == null)
			{
				ConsoleUtils.WriteLineColor("No response found for: RectangularPrismResponse!", ConsoleColor.Red);
				return;
			}

			var processTimeoutResponse = JsonSerializer.Deserialize<ProcessTimeoutResponse>(responseMessage.Body);

			ProcessTimeoutResponseHandler.Handle(processTimeoutResponse);

			await processServiceBusReceiver.DisposeAsync();
		}

		public async Task FinishJob()
		{
			await _sendOnlyServiceBusSender.DisposeAsync();

			await _sendAndReplyWaitServiceBusSender.DisposeAsync();

			await _sendAndReplyNoWaitServiceBusSender.DisposeAsync();

			await _sendAndReplyNoWaitServiceBusProcessor.StopProcessingAsync();
			await _sendAndReplyNoWaitServiceBusProcessor.DisposeAsync();

			await _serviceBusClient.DisposeAsync();
		}

		/// <summary>
		/// Response handler used to process rectangular prism response, process timeout response and exception response - No Wait
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		private async Task ResponseHandler(ProcessSessionMessageEventArgs arguments)
		{
			var body = arguments.Message.Body.ToString();

			var deserializedCorrectly = false;

			if (arguments.Message.Subject.Equals(MessageType.RectangularPrismResponse.GetDescription()))
			{
				var rectangularPrismResponse = JsonSerializer.Deserialize<RectangularPrismResponse>(body);
				deserializedCorrectly = RectangularPrismResponseHandler.Handle(rectangularPrismResponse);
			}
			else if (arguments.Message.Subject.Equals(MessageType.ProcessTimeoutResponse.GetDescription()))
			{
				var processTimeoutResponse = JsonSerializer.Deserialize<ProcessTimeoutResponse>(body);
				deserializedCorrectly = ProcessTimeoutResponseHandler.Handle(processTimeoutResponse);
			}
			else if (arguments.Message.Subject.Equals(MessageType.ExceptionResponse.GetDescription()))
			{
				var exceptionResponse = JsonSerializer.Deserialize<ExceptionResponse>(body);
				deserializedCorrectly = ExceptionResponseHandler.Handle(exceptionResponse);
			}

			// Complete message only if deserialization succeeded
			if (deserializedCorrectly)
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
