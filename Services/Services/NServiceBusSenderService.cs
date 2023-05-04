using Microsoft.Extensions.Configuration;
using NLog.Extensions.Logging;
using NServiceBus.Extensions.Logging;
using NServiceBus.Logging;
using Services.Contracts;
using Services.Models;
using Utils;

namespace Services.Services
{
	public class NServiceBusSenderService : ISenderService
	{
		private readonly IEndpointInstance _sendOnlyEndpointInstance;
		private readonly IEndpointInstance _sendAndReplyNoWaitEndpointInstance;

		public NServiceBusSenderService(IConfiguration configuration, bool isAzureServiceBus)
		{
			// Disable automatic NServiceBus logging
			LogManager.UseFactory(new ExtensionsLoggerFactory(new NLogLoggerFactory()));

			var sendOnlyEndpointConfiguration = new EndpointConfiguration(configuration.GetSection("ConnectionSettings")["SendOnlySenderEndpointName"]);
			var sendAndReplyNoWaitEndpointConfiguration = new EndpointConfiguration(configuration.GetSection("ConnectionSettings")["SendAndReplyNoWaitSenderEndpointName"]);

			if (isAzureServiceBus)
			{
				var sendOnlyTransport = sendOnlyEndpointConfiguration.UseTransport<AzureServiceBusTransport>();
				sendOnlyTransport.ConnectionString(configuration.GetConnectionString("AzureServiceBus"));
				sendOnlyTransport.Routing().RouteToEndpoint(typeof(SimpleMessage), configuration.GetSection("ConnectionSettings")["SendOnlyReceiverEndpointName"]);
				sendOnlyTransport.Routing().RouteToEndpoint(typeof(AdvancedMessage), configuration.GetSection("ConnectionSettings")["SendOnlyReceiverEndpointName"]);
				sendOnlyTransport.Routing().RouteToEndpoint(typeof(ExceptionMessage), configuration.GetSection("ConnectionSettings")["SendOnlyReceiverEndpointName"]);
				sendOnlyTransport.TopicName(configuration.GetSection("ConnectionSettings")["TopicName"]);
				sendOnlyEndpointConfiguration.SendOnly();

				var sendAndReplyTransport = sendAndReplyNoWaitEndpointConfiguration.UseTransport<AzureServiceBusTransport>();
				sendAndReplyTransport.ConnectionString(configuration.GetConnectionString("AzureServiceBus"));
				sendAndReplyTransport.Routing().RouteToEndpoint(typeof(RectangularPrismRequest), configuration.GetSection("ConnectionSettings")["SendAndReplyNoWaitReceiverEndpointName"]);
				sendAndReplyTransport.Routing().RouteToEndpoint(typeof(ProcessTimeoutRequest), configuration.GetSection("ConnectionSettings")["SendAndReplyNoWaitReceiverEndpointName"]);
				sendAndReplyTransport.TopicName(configuration.GetSection("ConnectionSettings")["TopicName"]);
			}
			else
			{
				var sendOnlyTransport = sendOnlyEndpointConfiguration.UseTransport<RabbitMQTransport>();
				sendOnlyTransport.UseConventionalRoutingTopology(QueueType.Quorum);
				sendOnlyTransport.ConnectionString($"host={configuration.GetSection("ConnectionSettings")["HostName"]}");
				sendOnlyTransport.Routing().RouteToEndpoint(typeof(SimpleMessage), configuration.GetSection("ConnectionSettings")["SendOnlyReceiverEndpointName"]);
				sendOnlyTransport.Routing().RouteToEndpoint(typeof(AdvancedMessage), configuration.GetSection("ConnectionSettings")["SendOnlyReceiverEndpointName"]);
				sendOnlyTransport.Routing().RouteToEndpoint(typeof(ExceptionMessage), configuration.GetSection("ConnectionSettings")["SendOnlyReceiverEndpointName"]);

				var sendAndReplyTransport = sendAndReplyNoWaitEndpointConfiguration.UseTransport<RabbitMQTransport>();
				sendAndReplyTransport.UseConventionalRoutingTopology(QueueType.Quorum);
				sendAndReplyTransport.ConnectionString($"host={configuration.GetSection("ConnectionSettings")["HostName"]}");
				sendAndReplyTransport.Routing().RouteToEndpoint(typeof(RectangularPrismRequest), configuration.GetSection("ConnectionSettings")["SendAndReplyNoWaitReceiverEndpointName"]);
				sendAndReplyTransport.Routing().RouteToEndpoint(typeof(ProcessTimeoutRequest), configuration.GetSection("ConnectionSettings")["SendAndReplyNoWaitReceiverEndpointName"]);
			}

			sendOnlyEndpointConfiguration.EnableInstallers();
			sendAndReplyNoWaitEndpointConfiguration.EnableInstallers();

			_sendOnlyEndpointInstance = Endpoint.Start(sendOnlyEndpointConfiguration).Result;
			_sendAndReplyNoWaitEndpointInstance = Endpoint.Start(sendAndReplyNoWaitEndpointConfiguration).Result;
		}

		public async Task SendSimpleMessage(SimpleMessage simpleMessage) => await _sendOnlyEndpointInstance.Send(simpleMessage);

		public async Task SendAdvancedMessage(AdvancedMessage advancedMessage) => await _sendOnlyEndpointInstance.Send(advancedMessage);

		public async Task SendExceptionMessage(ExceptionMessage exceptionMessage) => await _sendOnlyEndpointInstance.Send(exceptionMessage);

		public async Task SendAndReplyRectangularPrism(RectangularPrismRequest rectangularPrismRequest, bool wait)
		{
			// Wait solution was not found
			if (wait)
				return;

			await _sendAndReplyNoWaitEndpointInstance.Send(rectangularPrismRequest);
		}

		public async Task SendAndReplyProcessTimeout(ProcessTimeoutRequest processTimeoutRequest, bool wait)
		{
			// Wait solution was not found
			if (wait)
				return;

			await _sendAndReplyNoWaitEndpointInstance.Send(processTimeoutRequest);
		}

		public async Task FinishJob()
		{
			await _sendOnlyEndpointInstance.Stop();

			await _sendAndReplyNoWaitEndpointInstance.Stop();
		}
	}

	/// <summary>
	/// Handler class for rectangular prism response
	/// </summary>
	public class NServiceBusRectangularPrismResponseHandler : IHandleMessages<RectangularPrismResponse>
	{
		public async Task Handle(RectangularPrismResponse message, IMessageHandlerContext context)
		{
			await Task.Run(() =>
			{
				ConsoleUtils.WriteLineColor(message.ToString(), ConsoleColor.Green);
			}, context.CancellationToken);
		}
	}

	/// <summary>
	/// Handler class for process timeout response
	/// </summary>
	public class NServiceBusProcessTimeoutResponseHandler : IHandleMessages<ProcessTimeoutResponse>
	{
		public async Task Handle(ProcessTimeoutResponse message, IMessageHandlerContext context)
		{
			await Task.Run(() =>
			{
				ConsoleUtils.WriteLineColor($"Received process timeout response: {message.ProcessName}", ConsoleColor.Green);
			}, context.CancellationToken);
		}
	}

	/// <summary>
	/// Handler class for exception response
	/// </summary>
	public class NServiceBusExceptionResponseHandler : IHandleMessages<ExceptionResponse>
	{
		public async Task Handle(ExceptionResponse message, IMessageHandlerContext context)
		{
			await Task.Run(() =>
			{
				ConsoleUtils.WriteLineColor(message.Text, ConsoleColor.Red);
			}, context.CancellationToken);
		}
	}
}
