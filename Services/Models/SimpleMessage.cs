using Azure.Messaging.ServiceBus;
using System.Text;

namespace Services.Models
{
	public class SimpleMessage : IMessage
	{
		public string Text { get; set; } = string.Empty;
	}

	public static class SimpleMessageMapper
	{
		public static ServiceBusMessage ToServiceBusMessage(this SimpleMessage simpleMessage) => new ServiceBusMessage(simpleMessage.Text);
		public static byte[] ToRabbitMQMessage(this SimpleMessage simpleMessage) => Encoding.UTF8.GetBytes(simpleMessage.Text);
	}
}
