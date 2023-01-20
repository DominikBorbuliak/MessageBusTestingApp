using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.Contracts;
using Services.Models;
using Services.Services;
using Utils;

namespace Receiver
{
	public class Program
	{
		private static IConfiguration Configuration { get; set; } = null!;
		private static MessageBusType MessageBusType;

		private static void Main()
		{
			// Setup console
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.White;

			// Verificate message bus type
			var receiverTypeName = Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.MessageBusReceiverType);
			if (!Enum.TryParse(receiverTypeName, false, out MessageBusType))
			{
				ConsoleUtils.WriteLineColor($"Receiver type '{receiverTypeName}' is currently not supported!", ConsoleColor.Red);
				return;
			}

			Console.Title = $"{receiverTypeName} Receiver";

			// Build configuration file
			var builder = new ConfigurationBuilder()
				.AddJsonFile($"appsettings.{MessageBusType.GetConfigurationName()}.json", false, true);

			Configuration = builder.Build();

			// Convigure services
			IServiceCollection services = new ServiceCollection();
			ConfigureServices(services);

			var serviceProvider = services.BuildServiceProvider();
			var application = serviceProvider.GetService<Application>();

			// Run application
			application?.Run().Wait();
		}

		/// <summary>
		/// Initialize services for application
		/// </summary>
		/// <param name="services"></param>
		/// <exception cref="NotImplementedException"></exception>
		private static void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton(x => Configuration);

			switch (MessageBusType)
			{
				case MessageBusType.AzureServiceBus:
					services.AddSingleton<IReceiverService>(x => new AzureServiceBusReceiver(Configuration));
					break;
				case MessageBusType.RabbitMQ:
					services.AddSingleton<IReceiverService>(x => new RabbitMQReceiver(Configuration));
					break;
				case MessageBusType.NServiceBusRabbitMQ:
					services.AddSingleton<IReceiverService>(x => new NServiceBusRabbitMQReceiver(Configuration));
					break;
				case MessageBusType.NServiceBusAzureServiceBus:
					services.AddSingleton<IReceiverService>(x => new NServiceBusAzureServiceBusReceiver(Configuration));
					break;
				default:
					throw new NotImplementedException($"{MessageBusType} is not yet implemented!");
			}

			services.AddSingleton<Application>();
		}
	}
}