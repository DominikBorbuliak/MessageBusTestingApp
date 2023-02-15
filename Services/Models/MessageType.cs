using System.ComponentModel;

namespace Services.Models
{
	/// <summary>
	/// Enum used to determine which code should be triggered in receiver/sender
	/// </summary>
	public enum MessageType
	{
		[Description("SimpleMessage")]
		SimpleMessage,

		[Description("AdvancedMessage")]
		AdvancedMessage,

		[Description("RectangularPrismNoWaitRequest")]
		RectangularPrismNoWaitRequest,

		[Description("RectangularPrismWaitRequest")]
		RectangularPrismWaitRequest,

		[Description("RectangularPrismResponse")]
		RectangularPrismResponse,

		[Description("ProcessTimeoutNoWaitRequest")]
		ProcessTimeoutNoWaitRequest,

		[Description("ProcessTimeoutWaitRequest")]
		ProcessTimeoutWaitRequest,

		[Description("ProcessTimeoutResponse")]
		ProcessTimeoutResponse,

		[Description("ExceptionMessage")]
		ExceptionMessage,

		[Description("ExceptionResponse")]
		ExceptionResponse
	}
}
