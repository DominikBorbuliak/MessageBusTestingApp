using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Models;
using Utils;

namespace Services.Services
{
	public class NServiceBusReceiver : IReceiverService
	{
		private readonly EndpointConfiguration _endpointConfiguration;
		private IEndpointInstance _endpointInstance = null!;

		public NServiceBusReceiver(IConfiguration configuration, bool isAzureServiceBus)
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

		public async Task Run()
		{
			try
			{
				_endpointConfiguration.ExecuteTheseHandlersFirst(typeof(NServiceBusSimpleMessageHandler));
				_endpointConfiguration.ExecuteTheseHandlersFirst(typeof(NServiceBusAdvancedMessageHandler));

				_endpointInstance = await Endpoint.Start(_endpointConfiguration);

				Console.WriteLine("Press any key to exit application and stop processing!");
				Console.ReadKey();
			}
			finally
			{
				await _endpointInstance.Stop();
			}
		}
	}

	public class NServiceBusSimpleMessageHandler : IHandleMessages<SimpleMessage>
	{
		public Task Handle(SimpleMessage message, IMessageHandlerContext context)
		{
			ConsoleUtils.WriteLineColor($"Simple messsage received: {message.Text}", ConsoleColor.Green);
			return Task.CompletedTask;
		}
	}

	public class NServiceBusAdvancedMessageHandler : IHandleMessages<AdvancedMessage>
	{
		public Task Handle(AdvancedMessage message, IMessageHandlerContext context)
		{
			ConsoleUtils.WriteLineColor($"Advanced messsage received: {message}", ConsoleColor.Green);
			return Task.CompletedTask;
		}
	}
}
