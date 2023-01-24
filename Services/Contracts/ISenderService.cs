using Services.Models;

namespace Services.Contracts
{
	public interface ISenderService
	{
		/// <summary>
		/// Sends simple message to queue
		/// </summary>
		/// <param name="simpleMessage"></param>
		/// <returns></returns>
		Task SendSimpleMessage(SimpleMessage simpleMessage);

		/// <summary>
		/// Sends advanced message to queue
		/// </summary>
		/// <param name="advancedMessage"></param>
		/// <returns></returns>
		Task SendAdvancedMessage(AdvancedMessage advancedMessage);

		Task SendAndReplySimpleMessage(SimpleMessage simpleMessage);

		Task SendAndReplyAdvancedMessage(AdvancedMessage advancedMessage);

		/// <summary>
		/// Closes everything that was opened due sending of messages and clears memory
		/// </summary>
		/// <returns></returns>
		Task FinishJob();
	}
}
