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
				ConsoleUtils.WriteLineColor($"Advanced messsage received:\n{advancedMessage}", ConsoleColor.Green);
			}
			else if (arguments.Message.Subject.Equals(MessageType.ExceptionMessage.GetDescription()))
			{
				var exceptionMessage = JsonSerializer.Deserialize<ExceptionMessage>(body);

				if (exceptionMessage == null)
				{
					ConsoleUtils.WriteLineColor("No message found for: ExceptionMessage!", ConsoleColor.Red);
					return;
				}

				if (exceptionMessage.SucceedOn <= 0 || arguments.Message.DeliveryCount < exceptionMessage.SucceedOn)
				{
					ConsoleUtils.WriteLineColor($"Throwing exception with text: {exceptionMessage.ExceptionText}", ConsoleColor.Yellow);
					throw new Exception(exceptionMessage.ExceptionText);
				}

				ConsoleUtils.WriteLineColor($"Exception messsage with text: {exceptionMessage.ExceptionText} succeeded!", ConsoleColor.Green);
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

				if (rectangularPrismRequest == null)
				{
					ConsoleUtils.WriteLineColor("No request found for: RectangularPrismRequest!", ConsoleColor.Red);
					return;
				}

				if (rectangularPrismRequest.SucceedOn <= 0 || arguments.Message.DeliveryCount < rectangularPrismRequest.SucceedOn)
				{
					ConsoleUtils.WriteLineColor($"Throwing exception with text: {rectangularPrismRequest.ExceptionText}", ConsoleColor.Yellow);
					throw new Exception(rectangularPrismRequest.ExceptionText);
				}

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
			}
			else if (arguments.Message.Subject.Equals(MessageType.ProcessTimeoutRequest.GetDescription()))
			{
				var processTimeoutRequest = JsonSerializer.Deserialize<ProcessTimeoutRequest>(body);

				if (processTimeoutRequest == null)
				{
					ConsoleUtils.WriteLineColor("No request found for: ProcessTimeoutRequest!", ConsoleColor.Red);
					return;
				}

				ConsoleUtils.WriteLineColor($"Received process timeout request: {processTimeoutRequest.ProcessName}. Waiting for: {processTimeoutRequest.MillisecondsTimeout}ms", ConsoleColor.Green);
				await Task.Delay(processTimeoutRequest.MillisecondsTimeout);
				ConsoleUtils.WriteLineColor($"Sending process timeout response: {processTimeoutRequest.ProcessName}", ConsoleColor.Green);

				var processTimeoutResponse = new ProcessTimeoutResponse
				{
					ProcessName = processTimeoutRequest.ProcessName
				};

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
