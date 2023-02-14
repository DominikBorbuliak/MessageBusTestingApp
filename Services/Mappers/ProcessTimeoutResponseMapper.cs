using Azure.Messaging.ServiceBus;
using Services.Models;
using System.Text.Json;
using System.Text;

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
		/// <returns></returns>
		public static ServiceBusMessage ToServiceBusMessage(this ProcessTimeoutResponse processTimeoutResponse) => new(JsonSerializer.Serialize(processTimeoutResponse));

		/// <summary>
		/// Formats ProcessTimeoutResponse to RabbitMQ message
		/// </summary>
		/// <param name="processTimeoutResponse"></param>
		/// <returns></returns>
		public static byte[] ToRabbitMQMessage(this ProcessTimeoutResponse processTimeoutResponse) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(processTimeoutResponse));
	}
}
