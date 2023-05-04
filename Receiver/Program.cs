using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.Contracts;
using Services.Models;
using Services.Services;
using Services.View;
using Utils;

namespace Receiver
{
	public class Program
	{
		private static IConfiguration Configuration { get; set; } = null!;
		private static MessageBusType MessageBusType;

		private static void Main()
		{
			// Display intro screen and main menu
			var pickedMainMenuItem = Setup.Run("Receiver");

			// Exit was selected in main menu
			if (pickedMainMenuItem == null)
				return;

			MessageBusType = (MessageBusType)pickedMainMenuItem;

			// Build configuration file
			var builder = new ConfigurationBuilder()
				.AddJsonFile($"appsettings.{MessageBusType.GetConfigurationName()}.json", false, true);

			Configuration = builder.Build();

			// Configure services
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
					services.AddSingleton<IReceiverService>(x => new AzureServiceBusReceiverService(Configuration));
					break;
				case MessageBusType.RabbitMQ:
					services.AddSingleton<IReceiverService>(x => new RabbitMQReceiverService(Configuration));
					break;
				case MessageBusType.NServiceBusRabbitMQ:
					services.AddSingleton<IReceiverService>(x => new NServiceBusReceiverService(Configuration, false));
					break;
				case MessageBusType.NServiceBusAzureServiceBus:
					services.AddSingleton<IReceiverService>(x => new NServiceBusReceiverService(Configuration, true));
					break;
				default:
					throw new NotImplementedException($"{MessageBusType} is not implemented!");
			}

			services.AddSingleton<Application>();
		}
	}
}