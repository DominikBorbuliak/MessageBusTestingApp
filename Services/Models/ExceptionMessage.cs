using Azure.Messaging.ServiceBus;
using System.Text;
using System.Text.Json;
using Utils;

namespace Services.Models
{
	/// <summary>
	/// Model used to simulate thrown exception during processing
	/// </summary>
	public class ExceptionMessage : IMessage
	{
		public string ExceptionText { get; set; } = string.Empty;
		public int SucceedOn { get; set; }
	}

	/// <summary>
	/// Mapper class to format simple message to required format
	/// </summary>
	public static class ExceptionMessageMapper
	{
		public static ServiceBusMessage ToServiceBusMessage(this ExceptionMessage exceptionMessage) => new ServiceBusMessage(JsonSerializer.Serialize(exceptionMessage))
		{
			Subject = MessageType.ExceptionMessage.GetDescription()
		};

		public static byte[] ToRabbitMQMessage(this ExceptionMessage exceptionMessage) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(exceptionMessage));
	}
}
