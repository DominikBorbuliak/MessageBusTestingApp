using Sender.Models;
using Utils;

namespace Sender
{
	public class Program
	{
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
		}
	}
}