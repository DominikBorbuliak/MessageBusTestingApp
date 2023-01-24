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

		[MenuDisplayName("Send Only - N Random Messages - Simple")]
		SendOnlyNRandomSimpleMessages,

		[MenuDisplayName("Send Only - N Random Messages - Advanced")]
		SendOnlyNRandomAdvancedMessages,

		[MenuDisplayName("Send & Reply - 1 Custom Message - Simple")]
		SendAndReplyOneCustomSimpleMessage,

		[MenuDisplayName("Send & Reply - 1 Custom Message - Advanced")]
		SendAndReplyOneCustomAdvancedMessage,

		[MenuDisplayName("Send & Reply - N Random Messages - Simple")]
		SendAndReplyNRandomSimpleMessages,

		[MenuDisplayName("Send & Reply - N Random Messages - Advanced")]
		SendAndReplyNRandomAdvancedMessages
	}
}
