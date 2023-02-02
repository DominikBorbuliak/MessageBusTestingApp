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

		/// <summary>
		/// Sends exception message to simulate exception thrown during processing
		/// </summary>
		/// <param name="exceptionMessage"></param>
		/// <returns></returns>
		Task SendExceptionMessage(ExceptionMessage exceptionMessage);

		/// <summary>
		/// Sends rectangular prism request and waits for response
		/// </summary>
		/// <param name="rectangularPrismRequest"></param>
		/// <returns></returns>
		Task SendAndReplyRectangularPrism(RectangularPrismRequest rectangularPrismRequest);

		/// <summary>
		/// Sends process timeout request and waits for response
		/// </summary>
		/// <param name="processTimeoutRequest"></param>
		/// <returns></returns>
		Task SendAndReplyProcessTimeout(ProcessTimeoutRequest processTimeoutRequest);

		/// <summary>
		/// Closes everything that was opened due sending of messages and clears memory
		/// </summary>
		/// <returns></returns>
		Task FinishJob();
	}
}
