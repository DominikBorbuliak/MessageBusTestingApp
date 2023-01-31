using Services.Contracts;
using Utils;

namespace Receiver
{
	public class Application
	{
		private readonly IReceiverService _receiverService;

		public Application(IReceiverService receiverService)
		{
			_receiverService = receiverService;
		}

		/// <summary>
		/// Run the whole application
		/// </summary>
		/// <returns></returns>
		public async Task Run()
		{
			try
			{
				await _receiverService.StartJob();

				ConsoleKey key;
				do
				{
					Console.WriteLine("Press ESC to exit application or C to clear the console!");
					key = Console.ReadKey(true).Key;

					if (key == ConsoleKey.C)
						Console.Clear();

				} while (key != ConsoleKey.Escape);
			}
			catch
			{
				ConsoleUtils.WriteLineColor($"Error occured. Please read the readme file, to check if you have everything setup correctly.", ConsoleColor.Red);
				ConsoleUtils.WriteLineColor($"Feel free to contact administrator via email '514127@mail.muni.cz' if the problem persists.", ConsoleColor.Red);
				Console.WriteLine();
				ConsoleUtils.WriteLineColor($"Press anything to exit application...", ConsoleColor.Red);
				Console.ReadKey();
			}
			finally
			{
				await _receiverService.FinishJob();
			}
		}
	}
}
