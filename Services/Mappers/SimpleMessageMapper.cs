using Azure.Messaging.ServiceBus;
using Services.Models;
using System.Text;
using Utils;

namespace Services.Mappers
{
	/// <summary>
	/// Class used to map simple message
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
