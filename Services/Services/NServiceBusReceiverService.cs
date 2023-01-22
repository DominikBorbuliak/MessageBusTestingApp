using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Models;
using Utils;

namespace Services.Services
{
	public class NServiceBusReceiverService : IReceiverService
	{
		private readonly EndpointConfiguration _endpointConfiguration;
		private IEndpointInstance _endpointInstance = null!;

		public NServiceBusReceiverService(IConfiguration configuration, bool isAzureServiceBus)
		{
			_endpointConfiguration = new EndpointConfiguration(configuration.GetSection("ConnectionSettings")["ReceiverEndpointName"]);

			if (isAzureServiceBus)
			{
				var transport = _endpointConfiguration.UseTransport<AzureServiceBusTransport>();
				transport.ConnectionString(configuration.GetConnectionString("AzureServiceBus"));
				transport.TopicName(configuration.GetSection("ConnectionSettings")["TopicName"]);
			}
			else
			{
				var transport = _endpointConfiguration.UseTransport<RabbitMQTransport>();
				transport.UseConventionalRoutingTopology(QueueType.Quorum);
				transport.ConnectionString($"host={configuration.GetSection("ConnectionSettings")["HostName"]}");
			}

			_endpointConfiguration.EnableInstallers();
		}

		public async Task StartJob()
		{
			_endpointConfiguration.ExecuteTheseHandlersFirst(typeof(NServiceBusSimpleMessageHandler));
			_endpointConfiguration.ExecuteTheseHandlersFirst(typeof(NServiceBusAdvancedMessageHandler));

			_endpointInstance = await Endpoint.Start(_endpointConfiguration);
		}

		public async Task FinishJob()
		{
			await _endpointInstance.Stop();
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
