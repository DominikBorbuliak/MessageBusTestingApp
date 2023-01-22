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

				Console.WriteLine("Press any key to exit application and stop processing!");
				Console.ReadKey();
			}
			finally
			{
				await _receiverService.FinishJob();
			}
		}
	}
}
