namespace Services.Models
{
	/// <summary>
	/// Model used to simulate sending of simple text
	/// </summary>
	public class SimpleMessage : IMessage
	{
		public string Text { get; set; } = string.Empty;
	}
}
