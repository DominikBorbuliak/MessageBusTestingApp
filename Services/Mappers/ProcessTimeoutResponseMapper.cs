using Azure.Messaging.ServiceBus;
using Services.Models;
using System.Text;
using System.Text.Json;
using Utils;

namespace Services.Mappers
{
	/// <summary>
	/// Class used to map process timeout response
	/// </summary>
	public static class ProcessTimeoutResponseMapper
	{
		/// <summary>
		/// Maps ProcessTimeoutResponse to ServiceBusMessage
		/// </summary>
		/// <param name="processTimeoutResponse">Process timeout response to map</param>
		/// <param name="sessionId">Session id for current process timeout response</param>
		/// <returns></returns>
		public static ServiceBusMessage ToServiceBusMessage(this ProcessTimeoutResponse processTimeoutResponse, string sessionId) => new(JsonSerializer.Serialize(processTimeoutResponse))
		{
			Subject = MessageType.ProcessTimeoutResponse.GetDescription(),
			SessionId = sessionId
		};

		/// <summary>
		/// Maps ProcessTimeoutResponse to RabbitMQ message
		/// </summary>
		/// <param name="processTimeoutResponse">Process timeout response to map</param>
		/// <returns></returns>
		public static byte[] ToRabbitMQMessage(this ProcessTimeoutResponse processTimeoutResponse) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(processTimeoutResponse));
	}
}
