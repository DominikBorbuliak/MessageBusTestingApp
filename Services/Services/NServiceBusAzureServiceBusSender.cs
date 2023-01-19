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
			var endpointConfiguration = new EndpointConfiguration(configuration.GetSection("ServiceBusSettings")["SenderEndpointName"]);

			var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
			transport.ConnectionString(configuration.GetConnectionString("ServiceBusNamespaceConnectionString"));
			transport.Routing().RouteToEndpoint(typeof(Message), configuration.GetSection("ServiceBusSettings")["ReceiverEndpointName"]);
			transport.TopicName(configuration.GetSection("ServiceBusSettings")["TopicName"]);

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
