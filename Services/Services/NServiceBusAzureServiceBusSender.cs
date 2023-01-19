using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Models;

namespace Services.Services
{
	public class NServiceBusAzureServiceBusSender : ISenderService
	{
		private readonly IEndpointInstance _endpointInstance;

		public NServiceBusAzureServiceBusSender(IConfiguration configuration)
		{
			var endpointConfiguration = new EndpointConfiguration(configuration.GetSection("ConnectionSettings")["SenderEndpointName"]);

			var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
			transport.ConnectionString(configuration.GetConnectionString("AzureServiceBus"));
			transport.Routing().RouteToEndpoint(typeof(Message), configuration.GetSection("ConnectionSettings")["ReceiverEndpointName"]);
			transport.TopicName(configuration.GetSection("ConnectionSettings")["TopicName"]);

			endpointConfiguration.SendOnly();
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
