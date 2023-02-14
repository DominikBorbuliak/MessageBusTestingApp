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
		private readonly IEndpointInstance _sendAndReplyEndpointInstance;

		public NServiceBusSenderService(IConfiguration configuration, bool isAzureServiceBus)
		{
			// Disable automatic NServiceBus logging
			LogManager.UseFactory(new ExtensionsLoggerFactory(new NLogLoggerFactory()));

			var sendOnlyEndpointConfiguration = new EndpointConfiguration(configuration.GetSection("ConnectionSettings")["SendOnlySenderEndpointName"]);
			var sendAndReplyEndpointConfiguration = new EndpointConfiguration(configuration.GetSection("ConnectionSettings")["SendAndReplySenderEndpointName"]);

			if (isAzureServiceBus)
			{
				var sendOnlyTransport = sendOnlyEndpointConfiguration.UseTransport<AzureServiceBusTransport>();
				sendOnlyTransport.ConnectionString(configuration.GetConnectionString("AzureServiceBus"));
				sendOnlyTransport.Routing().RouteToEndpoint(typeof(SimpleMessage), configuration.GetSection("ConnectionSettings")["SendOnlyReceiverEndpointName"]);
				sendOnlyTransport.Routing().RouteToEndpoint(typeof(AdvancedMessage), configuration.GetSection("ConnectionSettings")["SendOnlyReceiverEndpointName"]);
				sendOnlyTransport.Routing().RouteToEndpoint(typeof(ExceptionMessage), configuration.GetSection("ConnectionSettings")["SendOnlyReceiverEndpointName"]);
				sendOnlyTransport.TopicName(configuration.GetSection("ConnectionSettings")["TopicName"]);
				sendOnlyEndpointConfiguration.SendOnly();

				var sendAndReplyTransport = sendAndReplyEndpointConfiguration.UseTransport<AzureServiceBusTransport>();
				sendAndReplyTransport.ConnectionString(configuration.GetConnectionString("AzureServiceBus"));
				sendAndReplyTransport.Routing().RouteToEndpoint(typeof(RectangularPrismRequest), configuration.GetSection("ConnectionSettings")["SendAndReplyReceiverEndpointName"]);
				sendAndReplyTransport.Routing().RouteToEndpoint(typeof(ProcessTimeoutRequest), configuration.GetSection("ConnectionSettings")["SendAndReplyReceiverEndpointName"]);
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

				var sendAndReplyTransport = sendAndReplyEndpointConfiguration.UseTransport<RabbitMQTransport>();
				sendAndReplyTransport.UseConventionalRoutingTopology(QueueType.Quorum);
				sendAndReplyTransport.ConnectionString($"host={configuration.GetSection("ConnectionSettings")["HostName"]}");
				sendAndReplyTransport.Routing().RouteToEndpoint(typeof(RectangularPrismRequest), configuration.GetSection("ConnectionSettings")["SendAndReplyReceiverEndpointName"]);
				sendAndReplyTransport.Routing().RouteToEndpoint(typeof(ProcessTimeoutRequest), configuration.GetSection("ConnectionSettings")["SendAndReplyReceiverEndpointName"]);
			}

			sendOnlyEndpointConfiguration.EnableInstallers();
			sendAndReplyEndpointConfiguration.EnableInstallers();

			_sendOnlyEndpointInstance = Endpoint.Start(sendOnlyEndpointConfiguration).Result;
			_sendAndReplyEndpointInstance = Endpoint.Start(sendAndReplyEndpointConfiguration).Result;
		}

		public async Task SendSimpleMessage(SimpleMessage simpleMessage) => await _sendOnlyEndpointInstance.Send(simpleMessage);

		public async Task SendAdvancedMessage(AdvancedMessage advancedMessage) => await _sendOnlyEndpointInstance.Send(advancedMessage);

		public async Task SendExceptionMessage(ExceptionMessage exceptionMessage) => await _sendOnlyEndpointInstance.Send(exceptionMessage);

		public async Task SendAndReplyRectangularPrism(RectangularPrismRequest rectangularPrismRequest) => await _sendAndReplyEndpointInstance.Send(rectangularPrismRequest);

		public async Task SendAndReplyProcessTimeout(ProcessTimeoutRequest processTimeoutRequest) => await _sendAndReplyEndpointInstance.Send(processTimeoutRequest);

		public async Task FinishJob()
		{
			await _sendOnlyEndpointInstance.Stop();

			await _sendAndReplyEndpointInstance.Stop();
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
