using Utils;

namespace Services.Models
{
	/// <summary>
	/// Enum used to determine which type of message bus should be used
	/// </summary>
	public enum MessageBusType
	{
		[ConfigurationName("azure-service-bus")]
		[MenuDisplayName("Azure Service Bus")]
		AzureServiceBus,

		[ConfigurationName("rabbit-mq")]
		[MenuDisplayName("Rabbit MQ")]
		RabbitMQ,

		[ConfigurationName("n-service-bus-azure-service-bus")]
		[MenuDisplayName("N Service Bus & Azure Service Bus")]
		NServiceBusAzureServiceBus,

		[ConfigurationName("n-service-bus-rabbit-mq")]
		[MenuDisplayName("N Service Bus & Rabbit MQ")]
		NServiceBusRabbitMQ
	}
}
