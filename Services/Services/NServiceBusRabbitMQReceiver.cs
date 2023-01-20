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
				_endpointConfiguration.ExecuteTheseHandlersFirst(typeof(NServiceBusRabbitMQHandler));

				_endpointInstance = await Endpoint.Start(_endpointConfiguration);

				Console.WriteLine("Press any key to exit application and stop processing!");
				Console.ReadKey();

				await _endpointInstance.Stop();
			}
			finally
			{

			}
		}
	}

	public class NServiceBusRabbitMQHandler : IHandleMessages<Message>
	{
		public Task Handle(Message message, IMessageHandlerContext context)
		{
			ConsoleUtils.WriteLineColor($"Messsage received: {message.Text}", ConsoleColor.Green);
			return Task.CompletedTask;
		}
	}
}
