using System.ComponentModel;

namespace Receiver.Models
{
	public enum ReceiverType
	{
		[Description("azure-service-bus")]
		AzureServiceBus,

		[Description("microsoft-biz-talk-server")]
		MicrosoftBizTalkServer,

		[Description("n-service-bus")]
		NServiceBus,

		[Description("rabbit-mq")]
		RabbitMQ
	}
}
