using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Models;

namespace Services.Services
{
	public class AzureServiceBusSender : ISenderService
	{
		private readonly ServiceBusClient _serviceBusClient;
		private readonly ServiceBusSender _serviceBusSender;

		public AzureServiceBusSender(IConfiguration configuration)
		{
			_serviceBusClient = new ServiceBusClient(configuration.GetConnectionString("AzureServiceBus"), new ServiceBusClientOptions()
			{
				TransportType = ServiceBusTransportType.AmqpWebSockets
			});

			_serviceBusSender = _serviceBusClient.CreateSender(configuration.GetSection("ConnectionSettings")["QueueName"]);
		}

		public async Task SendMessageAsync(Message message)
		{
			await _serviceBusSender.SendMessageAsync(new ServiceBusMessage(message.Text));
		}

		public async Task DisposeAsync()
		{
			await _serviceBusSender.DisposeAsync();
			await _serviceBusClient.DisposeAsync();
		}
	}
}
