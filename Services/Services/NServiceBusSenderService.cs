using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Models;

namespace Services.Services
{
	public class NServiceBusSenderService : ISenderService
	{
		private readonly IEndpointInstance _sendOnlyEndpointInstance;

		public NServiceBusSenderService(IConfiguration configuration, bool isAzureServiceBus)
		{
			var endpointConfiguration = new EndpointConfiguration(configuration.GetSection("ConnectionSettings")["SendOnlySenderEndpointName"]);

			if (isAzureServiceBus)
			{
				var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();

				transport.ConnectionString(configuration.GetConnectionString("AzureServiceBus"));
				transport.Routing().RouteToEndpoint(typeof(SimpleMessage), configuration.GetSection("ConnectionSettings")["SendOnlyReceiverEndpointName"]);
				transport.Routing().RouteToEndpoint(typeof(AdvancedMessage), configuration.GetSection("ConnectionSettings")["SendOnlyReceiverEndpointName"]);
				transport.TopicName(configuration.GetSection("ConnectionSettings")["TopicName"]);

				endpointConfiguration.SendOnly();
			}
			else
			{
				var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();

				transport.UseConventionalRoutingTopology(QueueType.Quorum);
				transport.ConnectionString($"host={configuration.GetSection("ConnectionSettings")["HostName"]}");
				transport.Routing().RouteToEndpoint(typeof(SimpleMessage), configuration.GetSection("ConnectionSettings")["SendOnlyReceiverEndpointName"]);
				transport.Routing().RouteToEndpoint(typeof(AdvancedMessage), configuration.GetSection("ConnectionSettings")["SendOnlyReceiverEndpointName"]);
			}

			endpointConfiguration.EnableInstallers();
			_sendOnlyEndpointInstance = Endpoint.Start(endpointConfiguration).Result;
		}

		public async Task SendSimpleMessage(SimpleMessage simpleMessage)
		{
			await _sendOnlyEndpointInstance.Send(simpleMessage);
		}

		public async Task SendAdvancedMessage(AdvancedMessage advancedMessage)
		{
			await _sendOnlyEndpointInstance.Send(advancedMessage);
		}

		public async Task SendAndReplyRectangularPrism(RectangularPrismRequest rectangularPrismRequest)
		{
		}

		public async Task FinishJob()
		{
			await _sendOnlyEndpointInstance.Stop();
		}
	}
}
