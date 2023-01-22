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
			Console.CursorVisible = false;
			Console.Title = "Receiver";

			// Display main menu
			Menu<MessageBusType> mainMenu = new Menu<MessageBusType>("Please select message bus receiver type", "Use arrow DOWN and UP to navigate through menu.\nPress ENTER to submit.", true);
			var pickedMainMenuItem = mainMenu.HandleMenuMovement();

			// Exit was selected
			if (pickedMainMenuItem == null)
				return;

			MessageBusType = (MessageBusType)pickedMainMenuItem;
			Console.Clear();
			Console.Title = $"{MessageBusType.GetMenuDisplayName()} Receiver";

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
					throw new NotImplementedException($"{MessageBusType} is not yet implemented!");
			}

			services.AddSingleton<Application>();
		}
	}
}