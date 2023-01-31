using Azure.Messaging.ServiceBus;
using System.Text.Json;
using System.Text;
using Utils;

namespace Services.Models
{
	public class ProcessTimeoutRequest : IMessage
	{
		public int MillisecondsTimeout { get; set; }
		public string ProcessName { get; set; } = string.Empty;
	}

	public static class ProcessTimeoutRequestMapper
	{
		public static ServiceBusMessage ToServiceBusMessage(this ProcessTimeoutRequest processTimeoutRequest) => new ServiceBusMessage(JsonSerializer.Serialize(processTimeoutRequest))
		{
			Subject = MessageType.ProcessTimeoutRequest.GetDescription()
		};

		public static byte[] ToRabbitMQMessage(this ProcessTimeoutRequest processTimeoutRequest) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(processTimeoutRequest));
	}
}
