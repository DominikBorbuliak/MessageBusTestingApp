using Services.Models;

namespace Services.Contracts
{
	/// <summary>
	/// Interface used for all sender services
	/// </summary>
	public interface ISenderService
	{
		/// <summary>
		/// Sends simple message to queue
		/// </summary>
		/// <param name="simpleMessage">Simple Message to send</param>
		/// <returns></returns>
		Task SendSimpleMessage(SimpleMessage simpleMessage);

		/// <summary>
		/// Sends advanced message to queue
		/// </summary>
		/// <param name="advancedMessage">Advanced Message to send</param>
		/// <returns></returns>
		Task SendAdvancedMessage(AdvancedMessage advancedMessage);

		/// <summary>
		/// Sends exception message to simulate exception thrown during processing - Send Only
		/// </summary>
		/// <param name="exceptionMessage">Exception Message to send</param>
		/// <returns></returns>
		Task SendExceptionMessage(ExceptionMessage exceptionMessage);

		/// <summary>
		/// Sends rectangular prism request and waits for response (based on parameter)
		/// Can be used to simulate exception thrown during processing - Send & Reply
		/// </summary>
		/// <param name="rectangularPrismRequest">Rectaingular Prism Request to send</param>
		/// <param name="wait">Determines whether method should wait for response</param>
		/// <returns></returns>
		Task SendAndReplyRectangularPrism(RectangularPrismRequest rectangularPrismRequest, bool wait);

		/// <summary>
		/// Sends process timeout request and waits for response (based on parameter)
		/// </summary>
		/// <param name="processTimeoutRequest">Process Timeout Request to send</param>
		/// <param name="wait">Determines whether method should wait for response</param>
		/// <returns></returns>
		Task SendAndReplyProcessTimeout(ProcessTimeoutRequest processTimeoutRequest, bool wait);

		/// <summary>
		/// Closes everything that was opened due to sending of messages and requests and clears memory
		/// </summary>
		/// <returns></returns>
		Task FinishJob();
	}
}
