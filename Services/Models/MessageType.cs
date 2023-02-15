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

		[Description("ExceptionMessage")]
		ExceptionMessage,

		[Description("RectangularPrismWaitRequest")]
		RectangularPrismWaitRequest,

		[Description("ProcessTimeoutWaitRequest")]
		ProcessTimeoutWaitRequest,

		[Description("RectangularPrismNoWaitRequest")]
		RectangularPrismNoWaitRequest,

		[Description("ProcessTimeoutNoWaitRequest")]
		ProcessTimeoutNoWaitRequest,

		[Description("RectangularPrismResponse")]
		RectangularPrismResponse,

		[Description("ProcessTimeoutResponse")]
		ProcessTimeoutResponse,

		[Description("ExceptionResponse")]
		ExceptionResponse
	}
}
