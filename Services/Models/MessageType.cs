using System.ComponentModel;

namespace Services.Models
{
	public enum MessageType
	{
		[Description("SimpleMessage")]
		SimpleMessage,

		[Description("AdvancedMessage")]
		AdvancedMessage
	}
}
