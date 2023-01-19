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

		public void SetupHandlers()
		{
			_endpointConfiguration.ExecuteTheseHandlersFirst(typeof(Handler));
		}

		public async Task StartProcessingAsync()
		{
			_endpointInstance = await Endpoint.Start(_endpointConfiguration);
		}

		public async Task StopProcessingAsync()
		{
			await _endpointInstance.Stop();
		}

		public Task DisposeAsync()
		{
			return Task.CompletedTask;
		}
	}

	public class Handler : IHandleMessages<Message>
	{
		public Task Handle(Message message, IMessageHandlerContext context)
		{
			ConsoleUtils.WriteLineColor($"Messsage received: {message.Text}", ConsoleColor.Green);
			return Task.CompletedTask;
		}
	}
}
