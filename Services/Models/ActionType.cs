using Utils;

namespace Services.Models
{
	/// <summary>
	/// Enum used to determine send action
	/// </summary>
	public enum ActionType
	{
		[MenuDisplayName("Send Only - 1 Custom Message - Simple")]
		SendOnlyOneCustomSimpleMessage,

		[MenuDisplayName("Send Only - 1 Custom Message - Advanced")]
		SendOnlyOneCustomAdvancedMessage,

		[MenuDisplayName("Send Only - N Custom Messages - Simple")]
		SendOnlyNRandomSimpleMessages,

		[MenuDisplayName("Send Only - N Custom Messages - Advanced")]
		SendOnlyNRandomAdvancedMessages
	}
}
