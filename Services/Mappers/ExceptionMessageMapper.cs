using Azure.Messaging.ServiceBus;
using Services.Models;
using System.Text;
using System.Text.Json;
using Utils;

namespace Services.Mappers
{
	/// <summary>
	/// Class used to map exception messages
	/// </summary>
	public static class ExceptionMessageMapper
	{
		/// <summary>
		/// Maps ExceptionMessage to ServiceBusMessage
		/// </summary>
		/// <param name="exceptionMessage"></param>
		/// <returns></returns>
		public static ServiceBusMessage ToServiceBusMessage(this ExceptionMessage exceptionMessage) => new(JsonSerializer.Serialize(exceptionMessage))
		{
			Subject = MessageType.ExceptionMessage.GetDescription()
		};

		/// <summary>
		/// Maps ExceptionMessage to RabbitMQ message
		/// </summary>
		/// <param name="exceptionMessage"></param>
		/// <returns></returns>
		public static byte[] ToRabbitMQMessage(this ExceptionMessage exceptionMessage) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(exceptionMessage));
	}
}
