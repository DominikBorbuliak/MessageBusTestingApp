using Azure.Messaging.ServiceBus;
using Services.Models;
using System.Text;
using System.Text.Json;

namespace Services.Mappers
{
	/// <summary>
	/// Class used to map exception response
	/// </summary>
	public static class ExceptionResponseMapper
	{
		/// <summary>
		/// Formats ExceptionResponse to RabbitMQ message
		/// </summary>
		/// <param name="exceptionResponse"></param>
		/// <returns></returns>
		public static byte[] ToRabbitMQMessage(this ExceptionResponse exceptionResponse) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(exceptionResponse));
	}
}
