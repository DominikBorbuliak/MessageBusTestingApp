using Azure.Messaging.ServiceBus;
using System.Text;
using Utils;

namespace Services.Models
{
	/// <summary>
	/// Model used to simulate sending of simple text
	/// </summary>
	public class SimpleMessage : IMessage
	{
		public string Text { get; set; } = string.Empty;
	}

	/// <summary>
	/// Mapper class to format simple message to required format
	/// </summary>
	public static class SimpleMessageMapper
	{
		/// <summary>
		/// Formats SimpleMessage to ServiceBusMessage
		/// </summary>
		/// <param name="simpleMessage"></param>
		/// <returns></returns>
		public static ServiceBusMessage ToServiceBusMessage(this SimpleMessage simpleMessage) => new(simpleMessage.Text)
		{
			Subject = MessageType.SimpleMessage.GetDescription()
		};

		/// <summary>
		/// Formats SimpleMessage to ServiceBusMessage
		/// </summary>
		/// <param name="simpleMessage"></param>
		/// <returns></returns>
		public static byte[] ToRabbitMQMessage(this SimpleMessage simpleMessage) => Encoding.UTF8.GetBytes(simpleMessage.Text);
	}
}
