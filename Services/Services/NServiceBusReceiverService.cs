using Microsoft.Extensions.Configuration;
using NLog.Extensions.Logging;
using NServiceBus.Extensions.Logging;
using NServiceBus.Logging;
using NServiceBus.Transport;
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
			// Disable automatic NServiceBus logging
			LogManager.UseFactory(new ExtensionsLoggerFactory(new NLogLoggerFactory()));

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

			var recoverability = _sendOnlyEndpointConfiguration.Recoverability();
			recoverability.CustomPolicy(ErrorHandler);

			var recoverabilitSendAndReply = _sendAndReplyEndpointConfiguration.Recoverability();
			recoverabilitSendAndReply.CustomPolicy(ErrorHandler);

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

		private RecoverabilityAction ErrorHandler(RecoverabilityConfig config, ErrorContext context)
		{
			ConsoleUtils.WriteLineColor($"Exception occured: {context.Exception.Message}", ConsoleColor.Red);

			return RecoverabilityAction.ImmediateRetry();
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
	/// Handler class for exception message
	/// </summary>
	public class NServiceBusExceptionMessageHandler : IHandleMessages<ExceptionMessage>
	{
		/// <summary>
		/// IMessageHandlerContext does not include delivery count property so we replace it with this property
		/// Property must be static as NServiceBus creates a new instance for each message handle attempt
		/// </summary>
		private static readonly IDictionary<string, int> _deliveryCounts = new Dictionary<string, int>();

		public async Task Handle(ExceptionMessage message, IMessageHandlerContext context)
		{
			await Task.Run(() =>
			{
				if (!_deliveryCounts.ContainsKey(context.MessageId))
					_deliveryCounts.Add(context.MessageId, 1);

				var deliveryCount = _deliveryCounts[context.MessageId];

				if (deliveryCount < message.SucceedOn)
				{
					ConsoleUtils.WriteLineColor($"Throwing exception with text: {message.ExceptionText}", ConsoleColor.Yellow);

					_deliveryCounts[context.MessageId] += 1;

					throw new Exception(message.ExceptionText);
				}

				ConsoleUtils.WriteLineColor($"Exception messsage with text: {message.ExceptionText} succeeded!", ConsoleColor.Green);
			}, context.CancellationToken);
		}
	}

	/// <summary>
	/// Handler class for rectangular prism request
	/// </summary>
	public class NServiceBusRectangularPrismRequestHandler : IHandleMessages<RectangularPrismRequest>
	{
		/// <summary>
		/// IMessageHandlerContext does not include delivery count property so we replace it with this property
		/// Property must be static as NServiceBus creates a new instance for each message handle attempt
		/// </summary>
		private static readonly IDictionary<string, int> _deliveryCounts = new Dictionary<string, int>();

		/// <summary>
		/// Simulate simple error handling logic with maximum number of delivery attempts
		/// </summary>
		private static readonly int _maxNumberOfDeliveryCounts = 10;

		public async Task Handle(RectangularPrismRequest rectangularPrismRequest, IMessageHandlerContext context)
		{
			if (!_deliveryCounts.ContainsKey(context.MessageId))
				_deliveryCounts.Add(context.MessageId, 1);

			var deliveryCount = _deliveryCounts[context.MessageId];
			_deliveryCounts[context.MessageId] += 1;

			if (_maxNumberOfDeliveryCounts <= deliveryCount)
			{
				await context.Reply(new ExceptionResponse { Text = "No response found for: RectangularPrismResponse!" });
				return;
			}

			var rectangularPrismResponse = RectangularPrismRequestHandler.HandleAndGenerateResponse(rectangularPrismRequest, deliveryCount);
			if (rectangularPrismResponse == null)
				return;

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
