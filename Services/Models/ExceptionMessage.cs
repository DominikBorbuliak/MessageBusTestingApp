using Utils;

namespace Services.Models
{
	/// <summary>
	/// Model used to simulate thrown exception during processing - Send Only
	/// </summary>
	public class ExceptionMessage : IMessage
	{
		/// <summary>
		/// Text of the exception that will be thrown in receiver
		/// </summary>
		public string ExceptionText { get; set; } = string.Empty;

		/// <summary>
		/// Number of attempt on which reciever should successfuly process message
		/// 0 or less - never
		/// 1 - first attempt
		/// </summary>
		public int SucceedOn { get; set; }
	}

	/// <summary>
	/// Handler class to handle exception message
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
