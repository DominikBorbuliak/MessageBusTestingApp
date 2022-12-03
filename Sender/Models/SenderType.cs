using System.ComponentModel;

namespace Sender.Models
{
	public enum SenderType
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