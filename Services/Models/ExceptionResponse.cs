namespace Services.Models
{
	/// <summary>
	/// Class that represents response message returned if exception occured in receiver in send and reply mode
	/// </summary>
	public class ExceptionResponse : IMessage
	{
		public string Text { get; set; } = string.Empty;
	}
}
