using System.ComponentModel;

namespace Services.Models
{
	public enum MessageBusType
	{
		[Description("azure-service-bus")]
		AzureServiceBus,

		[Description("rabbit-mq")]
		RabbitMQ,

		[Description("n-service-bus-azure-service-bus")]
		NServiceBusAzureServiceBus,

		[Description("n-service-bus-rabbit-mq")]
		NServiceBusRabbitMQ
	}
}
