using Services.Contracts;

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
			finally
			{
				await _receiverService.FinishJob();
			}
		}
	}
}
