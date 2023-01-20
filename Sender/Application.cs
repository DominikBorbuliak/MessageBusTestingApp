using Services.Contracts;

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
		public async Task Run() => await _senderService.Run();
	}
}
