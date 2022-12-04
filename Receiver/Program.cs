using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Receiver.Models;
using Utils;

namespace Receiver
{
	public class Program
	{
		private static IConfiguration Configuration { get; set; } = null!;

		private static void Main()
		{
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.White;

			var receiverTypeName = Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.MessageBusReceiverType);

			if (!Enum.TryParse<ReceiverType>(receiverTypeName, false, out var receiverType))
			{
				ConsoleUtils.WriteLineColor($"Receiver type '{receiverTypeName}' is currently not supported!", ConsoleColor.Red);
				return;
			}

			Console.WriteLine($"Hello from Receiver '{receiverType.GetDescription()}'");

			var builder = new ConfigurationBuilder()
				.AddJsonFile($"appsettings.{receiverType.GetDescription()}.json", false, true);

			Configuration = builder.Build();

			// TODO: Move to services and repositories
			var clientOptions = new ServiceBusClientOptions()
			{
				TransportType = ServiceBusTransportType.AmqpWebSockets
			};

			var serviceBusClient = new ServiceBusClient(Configuration.GetConnectionString("ServiceBusNamespaceConnectionString"), clientOptions);
			var serviceBusProcessor = serviceBusClient.CreateProcessor(Configuration.GetSection("ServiceBusSettings")["QueueName"], new ServiceBusProcessorOptions());

			try
			{
				serviceBusProcessor.ProcessMessageAsync += MessageHandler;
				serviceBusProcessor.ProcessErrorAsync += ErrorHandler;

				serviceBusProcessor.StartProcessingAsync().Wait();

				Console.WriteLine("Wait for a minute and then press any key to end the processing");
				Console.ReadKey();

				// stop processing 
				Console.WriteLine("\nStopping the receiver...");
				serviceBusProcessor.StopProcessingAsync().Wait();
				Console.WriteLine("Stopped receiving messages");
			}
			finally
			{
				serviceBusProcessor.DisposeAsync();
				serviceBusClient.DisposeAsync();
			}
		}

		private static async Task MessageHandler(ProcessMessageEventArgs arguments)
		{
			var body = arguments.Message.Body.ToString();

			Console.WriteLine(body);

			await arguments.CompleteMessageAsync(arguments.Message);
		}

		private static async Task ErrorHandler(ProcessErrorEventArgs args)
		{
			Console.WriteLine(args.Exception.ToString());
		}
	}
}