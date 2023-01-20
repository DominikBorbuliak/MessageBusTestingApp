using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Models;
using Utils;

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

		public async Task Run()
		{
			string? message;

			do
			{
				Console.WriteLine("Press enter to exit application or type text of the message!");
				message = Console.ReadLine();

				if (!string.IsNullOrEmpty(message))
				{
					await _endpointInstance.Send(new Message { Text = message });
					ConsoleUtils.WriteLineColor("Message was successfully send to queue!\n", ConsoleColor.Green);
				}
				else
				{
					ConsoleUtils.WriteLineColor("Application was successfully closed!", ConsoleColor.Green);
				}

			} while (!string.IsNullOrEmpty(message));

			await _endpointInstance.Stop();
		}
	}
}
