using System.ComponentModel;

namespace Services.Models
{
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
		ProcessTimeoutResponse
	}
}
