using Services.Contracts;
using Services.Models;

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
				_receiverService.SetupHandlers();
				await _receiverService.StartProcessingAsync();

				Console.WriteLine("Press any key to exit application and stop processing!");
				Console.ReadKey();

				await _receiverService.StopProcessingAsync();
			}
			finally
			{
				await _receiverService.DisposeAsync();
			}
		}
	}
}
