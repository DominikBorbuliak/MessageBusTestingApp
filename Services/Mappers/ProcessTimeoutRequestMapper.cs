using Azure.Messaging.ServiceBus;
using Services.Models;
using System.Text;
using System.Text.Json;
using Utils;

namespace Services.Mappers
{
	/// <summary>
	/// Class used to map process timeout request
	/// </summary>
	public static class ProcessTimeoutRequestMapper
	{
		/// <summary>
		/// Formats ProcessTimeoutRequest to ServiceBusMessage
		/// </summary>
		/// <param name="processTimeoutRequest"></param>
		/// <param name="sessionId"></param>
		/// <returns></returns>
		public static ServiceBusMessage ToServiceBusMessage(this ProcessTimeoutRequest processTimeoutRequest, string sessionId) => new(JsonSerializer.Serialize(processTimeoutRequest))
		{
			Subject = MessageType.ProcessTimeoutRequest.GetDescription(),
			SessionId = sessionId
		};

		/// <summary>
		/// Formats ProcessTimeoutRequest to RabbitMQ message
		/// </summary>
		/// <param name="processTimeoutRequest"></param>
		/// <returns></returns>
		public static byte[] ToRabbitMQMessage(this ProcessTimeoutRequest processTimeoutRequest) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(processTimeoutRequest));
	}
}
