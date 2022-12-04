using Services.Models;

namespace Services.Contracts
{
	public interface ISenderService
	{
		/// <summary>
		/// Sends a message to message bus
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		Task SendMessageAsync(Message message);

		/// <summary>
		/// Disposes everything that need to be disposed
		/// </summary>
		/// <returns></returns>
		Task DisposeAsync();
	}
}
