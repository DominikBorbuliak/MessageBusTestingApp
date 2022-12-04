using System.ComponentModel;

namespace Services.Models
{
	public enum MessageBusType
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
