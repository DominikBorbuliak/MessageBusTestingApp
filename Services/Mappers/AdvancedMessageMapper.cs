using Azure.Messaging.ServiceBus;
using Services.Models;
using System.Text;
using System.Text.Json;
using Utils;

namespace Services.Mappers
{
	/// <summary>
	/// Class used to map advanced messages
	/// </summary>
	public static class AdvancedMessageMapper
	{
		/// <summary>
		/// Maps AdvancedMessage to ServiceBusMessage
		/// </summary>
		/// <param name="advancedMessage"></param>
		/// <returns></returns>
		public static ServiceBusMessage ToServiceBusMessage(this AdvancedMessage advancedMessage) => new(JsonSerializer.Serialize(advancedMessage))
		{
			Subject = MessageType.AdvancedMessage.GetDescription()
		};

		/// <summary>
		/// Maps AdvancedMessage to RabbitMQ message
		/// </summary>
		/// <param name="advancedMessage"></param>
		/// <returns></returns>
		public static byte[] ToRabbitMQMessage(this AdvancedMessage advancedMessage) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(advancedMessage));
	}
}
