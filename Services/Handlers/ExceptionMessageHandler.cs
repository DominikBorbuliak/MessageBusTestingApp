using Services.Models;
using Utils;

namespace Services.Handlers
{
	/// <summary>
	/// Class used to handle exception message
	/// </summary>
	public static class ExceptionMessageHandler
	{
		/// <summary>
		/// Handles exception message
		/// Throws exception if simulating error
		/// </summary>
		/// <param name="exceptionMessage">Message to handle</param>
		/// <param name="deliveryCount">Current delivery attempt number</param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static bool Handle(ExceptionMessage? exceptionMessage, int deliveryCount)
		{
			if (exceptionMessage == null)
			{
				ConsoleUtils.WriteLineColor("ExceptionMessage could not be deserialized correctly!", ConsoleColor.Red);
				return false;
			}

			if (exceptionMessage.SucceedOn <= 0 || deliveryCount < exceptionMessage.SucceedOn)
			{
				ConsoleUtils.WriteLineColor($"Throwing exception with text: {exceptionMessage.ExceptionText}", ConsoleColor.Yellow);
				throw new Exception(exceptionMessage.ExceptionText);
			}

			ConsoleUtils.WriteLineColor($"Exception messsage with text: {exceptionMessage.ExceptionText} succeeded!", ConsoleColor.Green);

			return true;
		}
	}
}
