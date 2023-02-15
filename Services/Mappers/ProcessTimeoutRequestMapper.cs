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
		/// Maps ProcessTimeoutRequest to ServiceBusMessage
		/// </summary>
		/// <param name="processTimeoutRequest">Process timeout request to map</param>
		/// <param name="sessionId">Session id for current process timeout request</param>
		/// <param name="wait">Determines whether it is request to which sender must wait</param>
		/// <returns></returns>
		public static ServiceBusMessage ToServiceBusMessage(this ProcessTimeoutRequest processTimeoutRequest, string sessionId, bool wait) => new(JsonSerializer.Serialize(processTimeoutRequest))
		{
			Subject = wait ? MessageType.ProcessTimeoutWaitRequest.GetDescription() : MessageType.ProcessTimeoutNoWaitRequest.GetDescription(),
			SessionId = sessionId
		};

		/// <summary>
		/// Maps ProcessTimeoutRequest to RabbitMQ message
		/// </summary>
		/// <param name="processTimeoutRequest">Process timeout request to map</param>
		/// <returns></returns>
		public static byte[] ToRabbitMQMessage(this ProcessTimeoutRequest processTimeoutRequest) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(processTimeoutRequest));
	}
}
