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
		}
	}
}