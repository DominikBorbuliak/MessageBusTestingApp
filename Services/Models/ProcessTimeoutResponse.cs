using Azure.Messaging.ServiceBus;
using System.Text.Json;
using System.Text;

namespace Services.Models
{
	public class ProcessTimeoutResponse : IMessage
	{
		public string ProcessName { get; set; } = string.Empty;
	}

	public static class ProcessTimeoutResponseMapper
	{
		public static ServiceBusMessage ToServiceBusMessage(this ProcessTimeoutResponse processTimeoutResponse) => new ServiceBusMessage(JsonSerializer.Serialize(processTimeoutResponse));
		public static byte[] ToRabbitMQMessage(this ProcessTimeoutResponse processTimeoutResponse) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(processTimeoutResponse));
	}
}
