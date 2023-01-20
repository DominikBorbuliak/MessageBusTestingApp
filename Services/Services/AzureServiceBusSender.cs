using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Utils;

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

		public async Task Run()
		{
			string? message;

			do
			{
				Console.WriteLine("Press enter to exit application or type text of the message!");
				message = Console.ReadLine();

				if (!string.IsNullOrEmpty(message))
				{
					await _serviceBusSender.SendMessageAsync(new ServiceBusMessage(message));
					ConsoleUtils.WriteLineColor("Message was successfully send to queue!\n", ConsoleColor.Green);
				}
				else
				{
					ConsoleUtils.WriteLineColor("Application was successfully closed!", ConsoleColor.Green);
				}

			} while (!string.IsNullOrEmpty(message));

			await _serviceBusSender.DisposeAsync();
			await _serviceBusClient.DisposeAsync();
		}
	}
}
