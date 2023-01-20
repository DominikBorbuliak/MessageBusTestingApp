using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Models;
using Utils;

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
