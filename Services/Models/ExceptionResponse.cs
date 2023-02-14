namespace Services.Models
{
	/// <summary>
	/// Model that represents response message returned if exception occured in receiver in - Send & Reply
	/// </summary>
	public class ExceptionResponse : IMessage
	{
		public string Text { get; set; } = string.Empty;
	}
}
