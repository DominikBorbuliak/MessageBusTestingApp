using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Utils;

namespace Services.Services
{
	public class AzureServiceBusReceiver : IReceiverService
	{
		private readonly ServiceBusClient _serviceBusClient;
		private readonly ServiceBusProcessor _serviceBusProcessor;

		public AzureServiceBusReceiver(IConfiguration configuration)
		{
			_serviceBusClient = new ServiceBusClient(configuration.GetConnectionString("AzureServiceBus"), new ServiceBusClientOptions
			{
				TransportType = ServiceBusTransportType.AmqpWebSockets
			});

			_serviceBusProcessor = _serviceBusClient.CreateProcessor(configuration.GetSection("ConnectionSettings")["QueueName"], new ServiceBusProcessorOptions());
		}

		public async Task Run()
		{
			try
			{
				_serviceBusProcessor.ProcessMessageAsync += MessageHandler;
				_serviceBusProcessor.ProcessErrorAsync += ErrorHandler;

				await _serviceBusProcessor.StartProcessingAsync();

				Console.WriteLine("Press any key to exit application and stop processing!");
				Console.ReadKey();

				await _serviceBusProcessor.StopProcessingAsync();
			}
			finally
			{
				await _serviceBusProcessor.DisposeAsync();
				await _serviceBusClient.DisposeAsync();
			}
		}

		private async Task MessageHandler(ProcessMessageEventArgs arguments)
		{
			var body = arguments.Message.Body.ToString();

			ConsoleUtils.WriteLineColor($"Messsage received: {body}", ConsoleColor.Green);

			await arguments.CompleteMessageAsync(arguments.Message);
		}

		private async Task ErrorHandler(ProcessErrorEventArgs args)
		{
			ConsoleUtils.WriteLineColor($"Exception occured: {args.Exception}", ConsoleColor.Red);
		}
	}
}
