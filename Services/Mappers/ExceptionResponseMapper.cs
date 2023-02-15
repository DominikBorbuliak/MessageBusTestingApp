using Azure.Messaging.ServiceBus;
using Services.Models;
using System.Text;
using System.Text.Json;
using Utils;

namespace Services.Mappers
{
	/// <summary>
	/// Class used to map exception response
	/// </summary>
	public static class ExceptionResponseMapper
	{
		/// <summary>
		/// Maps ExceptionResponse to ServiceBusMessage
		/// </summary>
		/// <param name="exceptionResponse">Exception response to map</param>
		/// <param name="sessionId">Session id for current exception response</param>
		/// <returns></returns>
		public static ServiceBusMessage ToServiceBusMessage(this ExceptionResponse exceptionResponse, string sessionId) => new(JsonSerializer.Serialize(exceptionResponse))
		{
			Subject = MessageType.ExceptionResponse.GetDescription(),
			SessionId = sessionId
		};

		/// <summary>
		/// Maps ExceptionResponse to RabbitMQ message
		/// </summary>
		/// <param name="exceptionResponse">Exception response to map</param>
		/// <returns></returns>
		public static byte[] ToRabbitMQMessage(this ExceptionResponse exceptionResponse) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(exceptionResponse));
	}
}
