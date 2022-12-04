using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Sender.Models;
using Utils;

namespace Sender
{
	public class Program
	{
		private static IConfiguration Configuration { get; set; } = null!;

		private static void Main()
		{
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.White;

			var senderTypeName = Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.MessageBusSenderType);

			if (!Enum.TryParse<SenderType>(senderTypeName, false, out var senderType))
			{
				ConsoleUtils.WriteLineColor($"Sender type '{senderTypeName}' is currently not supported!", ConsoleColor.Red);
				return;
			}

			Console.WriteLine($"Hello from Sender '{senderType.GetDescription()}'");

			var builder = new ConfigurationBuilder()
				.AddJsonFile($"appsettings.{senderType.GetDescription()}.json", false, true);

			Configuration = builder.Build();

			// TODO: Move to services and repositories
			var clientOptions = new ServiceBusClientOptions()
			{
				TransportType = ServiceBusTransportType.AmqpWebSockets
			};

			var serviceBusClient = new ServiceBusClient(Configuration.GetConnectionString("ServiceBusNamespaceConnectionString"), clientOptions);
			var serviceBusSender = serviceBusClient.CreateSender(Configuration.GetSection("ServiceBusSettings")["QueueName"]);

			try
			{
				serviceBusSender.SendMessageAsync(new ServiceBusMessage("This is test message from SENDER!")).Wait();
			}
			finally
			{
				serviceBusSender.DisposeAsync();
				serviceBusClient.DisposeAsync();
			}
		}
	}
}