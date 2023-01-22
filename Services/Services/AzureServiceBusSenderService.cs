using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Models;

namespace Services.Services
{
	public class AzureServiceBusSenderService : ISenderService
	{
		private readonly ServiceBusClient _serviceBusClient;
		private readonly ServiceBusSender _serviceBusSender;

		public AzureServiceBusSenderService(IConfiguration configuration)
		{
			_serviceBusClient = new ServiceBusClient(configuration.GetConnectionString("AzureServiceBus"), new ServiceBusClientOptions()
			{
				TransportType = ServiceBusTransportType.AmqpWebSockets
			});

			_serviceBusSender = _serviceBusClient.CreateSender(configuration.GetSection("ConnectionSettings")["QueueName"]);
		}

		public async Task SendSimpleMessage(SimpleMessage simpleMessage)
		{
			await _serviceBusSender.SendMessageAsync(simpleMessage.ToServiceBusMessage());
		}

		public async Task SendAdvancedMessage(AdvancedMessage advancedMessage)
		{
			await _serviceBusSender.SendMessageAsync(advancedMessage.ToServiceBusMessage());
		}

		public async Task FinishJob()
		{
			await _serviceBusSender.DisposeAsync();
			await _serviceBusClient.DisposeAsync();
		}
	}
}
