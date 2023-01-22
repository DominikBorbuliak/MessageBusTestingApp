using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Models;
using Utils;

namespace Services.Services
{
	public class NServiceBusRabbitMQReceiver : IReceiverService
	{
		private readonly EndpointConfiguration _endpointConfiguration;
		private IEndpointInstance _endpointInstance = null!;

		public NServiceBusRabbitMQReceiver(IConfiguration configuration)
		{
			_endpointConfiguration = new EndpointConfiguration(configuration.GetSection("ConnectionSettings")["ReceiverEndpointName"]);

			var transport = _endpointConfiguration.UseTransport<RabbitMQTransport>();
			transport.UseConventionalRoutingTopology(QueueType.Quorum);
			transport.ConnectionString($"host={configuration.GetSection("ConnectionSettings")["HostName"]}");

			_endpointConfiguration.EnableInstallers();
		}

		public async Task Run()
		{
			try
			{
				_endpointConfiguration.ExecuteTheseHandlersFirst(typeof(NServiceBusRabbitMQSimpleMessageHandler));
				_endpointConfiguration.ExecuteTheseHandlersFirst(typeof(NServiceBusRabbitMQAdvancedMessageHandler));

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

	public class NServiceBusRabbitMQSimpleMessageHandler : IHandleMessages<SimpleMessage>
	{
		public Task Handle(SimpleMessage message, IMessageHandlerContext context)
		{
			ConsoleUtils.WriteLineColor($"Simple messsage received: {message.Text}", ConsoleColor.Green);
			return Task.CompletedTask;
		}
	}

	public class NServiceBusRabbitMQAdvancedMessageHandler : IHandleMessages<AdvancedMessage>
	{
		public Task Handle(AdvancedMessage message, IMessageHandlerContext context)
		{
			ConsoleUtils.WriteLineColor($"Advanced messsage received: {message}", ConsoleColor.Green);
			return Task.CompletedTask;
		}
	}
}
