using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Models;
using Utils;

namespace Services.Services
{
	public class NServiceBusReceiverService : IReceiverService
	{
		private readonly EndpointConfiguration _sendOnlyEndpointConfiguration;
		private IEndpointInstance _sendOnlyEndpointInstance = null!;

		private readonly EndpointConfiguration _sendAndReplyEndpointConfiguration;
		private IEndpointInstance _sendAndReplyEndpointInstance = null!;

		public NServiceBusReceiverService(IConfiguration configuration, bool isAzureServiceBus)
		{
			_sendOnlyEndpointConfiguration = new EndpointConfiguration(configuration.GetSection("ConnectionSettings")["SendOnlyReceiverEndpointName"]);
			_sendAndReplyEndpointConfiguration = new EndpointConfiguration(configuration.GetSection("ConnectionSettings")["SendAndReplyReceiverEndpointName"]);

			if (isAzureServiceBus)
			{
				var sendOnlyTransport = _sendOnlyEndpointConfiguration.UseTransport<AzureServiceBusTransport>();
				sendOnlyTransport.ConnectionString(configuration.GetConnectionString("AzureServiceBus"));
				sendOnlyTransport.TopicName(configuration.GetSection("ConnectionSettings")["TopicName"]);

				var sendAndReplyTransport = _sendAndReplyEndpointConfiguration.UseTransport<AzureServiceBusTransport>();
				sendAndReplyTransport.ConnectionString(configuration.GetConnectionString("AzureServiceBus"));
				sendAndReplyTransport.TopicName(configuration.GetSection("ConnectionSettings")["TopicName"]);
			}
			else
			{
				var sendOnlyTransport = _sendOnlyEndpointConfiguration.UseTransport<RabbitMQTransport>();
				sendOnlyTransport.UseConventionalRoutingTopology(QueueType.Quorum);
				sendOnlyTransport.ConnectionString($"host={configuration.GetSection("ConnectionSettings")["HostName"]}");

				var sendAndReplyTransport = _sendAndReplyEndpointConfiguration.UseTransport<RabbitMQTransport>();
				sendAndReplyTransport.UseConventionalRoutingTopology(QueueType.Quorum);
				sendAndReplyTransport.ConnectionString($"host={configuration.GetSection("ConnectionSettings")["HostName"]}");
			}

			_sendOnlyEndpointConfiguration.EnableInstallers();
			_sendAndReplyEndpointConfiguration.EnableInstallers();
		}

		public async Task StartJob()
		{
			_sendOnlyEndpointConfiguration.ExecuteTheseHandlersFirst(typeof(NServiceBusSimpleMessageHandler));
			_sendOnlyEndpointConfiguration.ExecuteTheseHandlersFirst(typeof(NServiceBusAdvancedMessageHandler));

			_sendOnlyEndpointInstance = await Endpoint.Start(_sendOnlyEndpointConfiguration);

			_sendAndReplyEndpointConfiguration.ExecuteTheseHandlersFirst(typeof(NServiceBusRectangularPrismRequestHandler));

			_sendAndReplyEndpointInstance = await Endpoint.Start(_sendAndReplyEndpointConfiguration);
		}

		public async Task FinishJob()
		{
			await _sendOnlyEndpointInstance.Stop();

			await _sendAndReplyEndpointInstance.Stop();
		}
	}

	/// <summary>
	/// Handler class for simple message
	/// </summary>
	public class NServiceBusSimpleMessageHandler : IHandleMessages<SimpleMessage>
	{
		public async Task Handle(SimpleMessage message, IMessageHandlerContext context)
		{
			await Task.Run(() =>
			{
				ConsoleUtils.WriteLineColor($"Simple messsage received: {message.Text}", ConsoleColor.Green);
			}, context.CancellationToken);
		}
	}

	/// <summary>
	/// Handler class for advanced message
	/// </summary>
	public class NServiceBusAdvancedMessageHandler : IHandleMessages<AdvancedMessage>
	{
		public async Task Handle(AdvancedMessage message, IMessageHandlerContext context)
		{
			await Task.Run(() =>
			{
				ConsoleUtils.WriteLineColor($"Advanced messsage received: {message}", ConsoleColor.Green);
			}, context.CancellationToken);
		}
	}

	/// <summary>
	/// Handler class for rectangular prism request
	/// </summary>
	public class NServiceBusRectangularPrismRequestHandler : IHandleMessages<RectangularPrismRequest>
	{
		public async Task Handle(RectangularPrismRequest rectangularPrismRequest, IMessageHandlerContext context)
		{
			ConsoleUtils.WriteLineColor($"Rectangular prism request received:\n{rectangularPrismRequest}", ConsoleColor.Green);

			var rectangularPrismResponse = new RectangularPrismResponse
			{
				SurfaceArea = 2 * (rectangularPrismRequest.EdgeA * rectangularPrismRequest.EdgeB + rectangularPrismRequest.EdgeA * rectangularPrismRequest.EdgeC + rectangularPrismRequest.EdgeB * rectangularPrismRequest.EdgeC),
				Volume = rectangularPrismRequest.EdgeA * rectangularPrismRequest.EdgeB * rectangularPrismRequest.EdgeC
			};

			ConsoleUtils.WriteLineColor("Sending rectangular prism response", ConsoleColor.Green);

			await context.Reply(rectangularPrismResponse);
		}
	}

	/// <summary>
	/// Handler class for process timeout request
	/// </summary>
	public class NServiceBusProcessTimeoutRequestHandler : IHandleMessages<ProcessTimeoutRequest>
	{
		public async Task Handle(ProcessTimeoutRequest processTimeoutRequest, IMessageHandlerContext context)
		{
			ConsoleUtils.WriteLineColor($"Received process timeout request: {processTimeoutRequest.ProcessName}. Waiting for: {processTimeoutRequest.MillisecondsTimeout}ms", ConsoleColor.Green);
			await Task.Delay(processTimeoutRequest.MillisecondsTimeout, context.CancellationToken);
			ConsoleUtils.WriteLineColor($"Sending process timeout response: {processTimeoutRequest.ProcessName}", ConsoleColor.Green);

			var processTimeoutResponse = new ProcessTimeoutResponse
			{
				ProcessName = processTimeoutRequest.ProcessName
			};

			await context.Reply(processTimeoutResponse);
		}
	}
}
