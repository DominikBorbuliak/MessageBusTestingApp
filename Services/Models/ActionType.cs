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

		[MenuDisplayName("Send Only - Simulate Exception Thrown in Receiver")]
		SendOnlySimulateException,

		[MenuDisplayName("Send & Reply - Wait - Surface area and Volume of Rectangular Prism")]
		SendAndReplyWaitRectangularPrism,

		[MenuDisplayName("Send & Reply - Wait - Simulate N Clients")]
		SendAndReplyWaitSimulateNClients,

		[MenuDisplayName("Send & Reply - Wait - Simulate Exception Thrown in Receiver")]
		SendAndReplyWaitSimulateException,

		[MenuDisplayName("Send & Reply - No Wait - Surface area and Volume of Rectangular Prism ")]
		SendAndReplyNoWaitRectangularPrism,

		[MenuDisplayName("Send & Reply - No Wait - Simulate N Clients")]
		SendAndReplyNoWaitSimulateNClients,

		[MenuDisplayName("Send & Reply - No Wait - Simulate Exception Thrown in Receiver")]
		SendAndReplyNoWaitSimulateException,
	}
}
