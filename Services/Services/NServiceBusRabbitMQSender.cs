using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Models;

namespace Services.Services
{
	public class NServiceBusRabbitMQSender : ISenderService
	{
		private readonly IEndpointInstance _endpointInstance;

		public NServiceBusRabbitMQSender(IConfiguration configuration)
		{
			var endpointConfiguration = new EndpointConfiguration(configuration.GetSection("ConnectionSettings")["SenderEndpointName"]);
			var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
			transport.UseConventionalRoutingTopology(QueueType.Quorum);
			transport.ConnectionString($"host={configuration.GetSection("ConnectionSettings")["HostName"]}");

			transport.Routing().RouteToEndpoint(typeof(Message), configuration.GetSection("ConnectionSettings")["ReceiverEndpointName"]);
			endpointConfiguration.EnableInstallers();

			_endpointInstance = Endpoint.Start(endpointConfiguration).Result;
		}

		public async Task SendMessageAsync(Message message)
		{
			await _endpointInstance.Send(message);
		}

		public async Task DisposeAsync()
		{
			await _endpointInstance.Stop();
		}
	}
}
