using System.ComponentModel;

namespace Services.Models
{
	/// <summary>
	/// Enum used to determine which code should be triggered in receiver
	/// </summary>
	public enum MessageType
	{
		[Description("SimpleMessage")]
		SimpleMessage,

		[Description("AdvancedMessage")]
		AdvancedMessage,

		[Description("RectangularPrismRequest")]
		RectangularPrismRequest,

		[Description("RectangularPrismResponse")]
		RectangularPrismResponse,

		[Description("ProcessTimeoutRequest")]
		ProcessTimeoutRequest,

		[Description("ProcessTimeoutResponse")]
		ProcessTimeoutResponse,

		[Description("ExceptionMessage")]
		ExceptionMessage
	}
}
