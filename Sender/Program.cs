using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.Contracts;
using Services.Models;
using Services.Services;
using Utils;

namespace Sender
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
			Console.Title = "Sender";

			// Display main menu
			Menu<MessageBusType> mainMenu = new Menu<MessageBusType>("Please select message bus sender type", "Use arrow DOWN and UP to navigate through menu.\nPress ENTER to submit.", true);
			var pickedMainMenuItem = mainMenu.HandleMenuMovement();

			// Exit was selected
			if (pickedMainMenuItem == null)
				return;

			MessageBusType = (MessageBusType)pickedMainMenuItem;
			Console.Clear();
			Console.Title = $"{MessageBusType.GetMenuDisplayName()} Sender";

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
					services.AddSingleton<ISenderService>(x => new AzureServiceBusSender(Configuration));
					break;
				case MessageBusType.RabbitMQ:
					services.AddSingleton<ISenderService>(x => new RabbitMQSender(Configuration));
					break;
				case MessageBusType.NServiceBusRabbitMQ:
					services.AddSingleton<ISenderService>(x => new NServiceBusRabbitMQSender(Configuration));
					break;
				case MessageBusType.NServiceBusAzureServiceBus:
					services.AddSingleton<ISenderService>(x => new NServiceBusAzureServiceBusSender(Configuration));
					break;
				default:
					throw new NotImplementedException($"{MessageBusType} is not yet implemented!");
			}

			services.AddSingleton<Application>();
		}
	}
}