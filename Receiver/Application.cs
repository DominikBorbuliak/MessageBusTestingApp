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
		public async Task Run() => await _receiverService.Run();
	}
}
