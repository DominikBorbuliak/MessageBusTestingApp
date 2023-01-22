using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Models;

namespace Services.Services
{
	public class NServiceBusSenderService : ISenderService
	{
		private readonly IEndpointInstance _endpointInstance;

		public NServiceBusSenderService(IConfiguration configuration, bool isAzureServiceBus)
		{
			var endpointConfiguration = new EndpointConfiguration(configuration.GetSection("ConnectionSettings")["SenderEndpointName"]);

			if (isAzureServiceBus)
			{
				var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();

				transport.ConnectionString(configuration.GetConnectionString("AzureServiceBus"));
				transport.Routing().RouteToEndpoint(typeof(SimpleMessage), configuration.GetSection("ConnectionSettings")["ReceiverEndpointName"]);
				transport.Routing().RouteToEndpoint(typeof(AdvancedMessage), configuration.GetSection("ConnectionSettings")["ReceiverEndpointName"]);
				transport.TopicName(configuration.GetSection("ConnectionSettings")["TopicName"]);

				endpointConfiguration.SendOnly();
			}
			else
			{
				var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();

				transport.UseConventionalRoutingTopology(QueueType.Quorum);
				transport.ConnectionString($"host={configuration.GetSection("ConnectionSettings")["HostName"]}");
				transport.Routing().RouteToEndpoint(typeof(SimpleMessage), configuration.GetSection("ConnectionSettings")["ReceiverEndpointName"]);
				transport.Routing().RouteToEndpoint(typeof(AdvancedMessage), configuration.GetSection("ConnectionSettings")["ReceiverEndpointName"]);
			}

			endpointConfiguration.EnableInstallers();
			_endpointInstance = Endpoint.Start(endpointConfiguration).Result;
		}

		public async Task SendSimpleMessage(SimpleMessage simpleMessage)
		{
			await _endpointInstance.Send(simpleMessage);
		}

		public async Task SendAdvancedMessage(AdvancedMessage advancedMessage)
		{
			await _endpointInstance.Send(advancedMessage);
		}

		public async Task FinishJob()
		{
			await _endpointInstance.Stop();
		}
	}
}
