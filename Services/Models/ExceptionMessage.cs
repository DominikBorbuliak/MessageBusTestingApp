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
}
