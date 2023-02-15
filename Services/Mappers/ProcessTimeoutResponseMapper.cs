using Azure.Messaging.ServiceBus;
using Services.Models;
using System.Text.Json;
using System.Text;
using Utils;

namespace Services.Mappers
{
	/// <summary>
	/// Class used to map process timeout response
	/// </summary>
	public static class ProcessTimeoutResponseMapper
	{
		/// <summary>
		/// Formats ProcessTimeoutResponse to ServiceBusMessage
		/// </summary>
		/// <param name="processTimeoutResponse"></param>
		/// <param name="sessionId"></param>
		/// <returns></returns>
		public static ServiceBusMessage ToServiceBusMessage(this ProcessTimeoutResponse processTimeoutResponse, string sessionId) => new(JsonSerializer.Serialize(processTimeoutResponse))
		{
			Subject = MessageType.ProcessTimeoutResponse.GetDescription(),
			SessionId = sessionId
		};

		/// <summary>
		/// Formats ProcessTimeoutResponse to RabbitMQ message
		/// </summary>
		/// <param name="processTimeoutResponse"></param>
		/// <returns></returns>
		public static byte[] ToRabbitMQMessage(this ProcessTimeoutResponse processTimeoutResponse) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(processTimeoutResponse));
	}
}
