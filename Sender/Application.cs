using Services.Contracts;
using Services.Models;
using Utils;

namespace Sender
{
	public class Application
	{
		private readonly ISenderService _senderService;

		public Application(ISenderService senderService)
		{
			_senderService = senderService;
		}

		/// <summary>
		/// Run the whole application
		/// </summary>
		/// <returns></returns>
		public async Task Run()
		{
			try
			{
				string? message;

				do
				{
					Console.WriteLine("Press enter to exit application or type text of the message!");
					message = Console.ReadLine();

					if (!string.IsNullOrEmpty(message))
					{
						await _senderService.SendMessageAsync(new Message { Text = message });
						ConsoleUtils.WriteLineColor("Message was successfully send to queue!\n", ConsoleColor.Green);
					}
					else
					{
						ConsoleUtils.WriteLineColor("Application was successfully closed!", ConsoleColor.Green);
					}

				} while (!string.IsNullOrEmpty(message));
			}
			finally
			{
				await _senderService.DisposeAsync();
			}
		}
	}
}
