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
			_sendAndReplyNoWaitServiceBusProcessor.ProcessErrorAsync += (args) => Task.CompletedTask;
			_sendAndReplyNoWaitServiceBusProcessor.StartProcessingAsync().Wait();
		}

		public async Task SendSimpleMessage(SimpleMessage simpleMessage) => await _sendOnlyServiceBusSender.SendMessageAsync(simpleMessage.ToServiceBusMessage());

		public async Task SendAdvancedMessage(AdvancedMessage advancedMessage) => await _sendOnlyServiceBusSender.SendMessageAsync(advancedMessage.ToServiceBusMessage());

		public async Task SendExceptionMessage(ExceptionMessage exceptionMessage) => await _sendOnlyServiceBusSender.SendMessageAsync(exceptionMessage.ToServiceBusMessage());

		public async Task SendAndReplyRectangularPrism(RectangularPrismRequest rectangularPrismRequest, bool wait)
		{
			var sessionId = Guid.NewGuid().ToString();
			var serviceBusMessage = rectangularPrismRequest.ToServiceBusMessage(sessionId, wait);

			if (!wait)
			{
				await _sendAndReplyNoWaitServiceBusSender.SendMessageAsync(serviceBusMessage);
				return;
			}

			var serviceBusReceiver = await _serviceBusClient.AcceptSessionAsync(_configuration.GetSection("ConnectionSettings")["SendAndReplyWaitSenderQueueName"], sessionId);

			await _sendAndReplyWaitServiceBusSender.SendMessageAsync(serviceBusMessage);

			var responseMessage = await serviceBusReceiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));

			if (responseMessage == null)
			{
				ConsoleUtils.WriteLineColor("No response found for: RectangularPrismResponse!", ConsoleColor.Red);
				return;
			}

			var response = JsonSerializer.Deserialize<RectangularPrismResponse>(responseMessage.Body);

			if (response != null)
				ConsoleUtils.WriteLineColor(response.ToString(), ConsoleColor.Green);
			else
				ConsoleUtils.WriteLineColor("Deserialization error: RectangularPrismResponse!", ConsoleColor.Red);

			await serviceBusReceiver.DisposeAsync();
		}

		public async Task SendAndReplyProcessTimeout(ProcessTimeoutRequest processTimeoutRequest, bool wait)
		{
			var processSesionId = Guid.NewGuid().ToString();
			var serviceBusMessage = processTimeoutRequest.ToServiceBusMessage(processSesionId, wait);

			if (!wait)
			{
				await _sendAndReplyNoWaitServiceBusSender.SendMessageAsync(serviceBusMessage);
				return;
			}

			var processServiceBusReceiver = await _serviceBusClient.AcceptSessionAsync(_configuration.GetSection("ConnectionSettings")["SendAndReplyWaitSenderQueueName"], processSesionId);

			await _sendAndReplyWaitServiceBusSender.SendMessageAsync(serviceBusMessage);

			var responseMessage = await processServiceBusReceiver.ReceiveMessageAsync();
			var response = JsonSerializer.Deserialize<ProcessTimeoutResponse>(responseMessage.Body);

			if (response != null)
				ConsoleUtils.WriteLineColor($"Received process timeout response: {response.ProcessName}", ConsoleColor.Green);
			else
				ConsoleUtils.WriteLineColor("No response found for: ProcessTimeoutResponse!", ConsoleColor.Red);

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
		/// Response handler used to process rectangular prism response
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		private async Task ResponseHandler(ProcessSessionMessageEventArgs arguments)
		{
			var body = arguments.Message.Body.ToString();

			if (arguments.Message.Subject.Equals(MessageType.RectangularPrismResponse.GetDescription()))
			{
				var rectangularPrismResponse = JsonSerializer.Deserialize<RectangularPrismResponse>(body);

				if (!RectangularPrismResponseHandler.Handle(rectangularPrismResponse))
					return;
			}
			else if (arguments.Message.Subject.Equals(MessageType.ProcessTimeoutResponse.GetDescription()))
			{
				var processTimeoutResponse = JsonSerializer.Deserialize<ProcessTimeoutResponse>(body);

				if (!ProcessTimeoutResponseHandler.Handle(processTimeoutResponse))
					return;
			}
			else if (arguments.Message.Subject.Equals(MessageType.ExceptionResponse.GetDescription()))
			{
				var exceptionResponse = JsonSerializer.Deserialize<ExceptionResponse>(body);

				if (!ExceptionResponseHandler.Handle(exceptionResponse))
					return;
			}

			await arguments.CompleteMessageAsync(arguments.Message);
		}
	}
}
