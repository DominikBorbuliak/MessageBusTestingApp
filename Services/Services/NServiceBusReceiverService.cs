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

		public NServiceBusReceiverService(IConfiguration configuration, bool isAzureServiceBus)
		{
			_sendOnlyEndpointConfiguration = new EndpointConfiguration(configuration.GetSection("ConnectionSettings")["SendOnlyReceiverEndpointName"]);

			if (isAzureServiceBus)
			{
				var transport = _sendOnlyEndpointConfiguration.UseTransport<AzureServiceBusTransport>();
				transport.ConnectionString(configuration.GetConnectionString("AzureServiceBus"));
				transport.TopicName(configuration.GetSection("ConnectionSettings")["TopicName"]);
			}
			else
			{
				var transport = _sendOnlyEndpointConfiguration.UseTransport<RabbitMQTransport>();
				transport.UseConventionalRoutingTopology(QueueType.Quorum);
				transport.ConnectionString($"host={configuration.GetSection("ConnectionSettings")["HostName"]}");
			}

			_sendOnlyEndpointConfiguration.EnableInstallers();
		}

		public async Task StartJob()
		{
			_sendOnlyEndpointConfiguration.ExecuteTheseHandlersFirst(typeof(NServiceBusSimpleMessageHandler));
			_sendOnlyEndpointConfiguration.ExecuteTheseHandlersFirst(typeof(NServiceBusAdvancedMessageHandler));

			_sendOnlyEndpointInstance = await Endpoint.Start(_sendOnlyEndpointConfiguration);
		}

		public async Task FinishJob()
		{
			await _sendOnlyEndpointInstance.Stop();
		}
	}

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
}
