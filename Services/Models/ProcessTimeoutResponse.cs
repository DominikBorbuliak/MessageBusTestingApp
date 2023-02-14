using Azure.Messaging.ServiceBus;
using System.Text;
using System.Text.Json;

namespace Services.Models
{
	/// <summary>
	/// Model used to simulate N concurent clients
	/// </summary>
	public class ProcessTimeoutResponse : IMessage
	{
		/// <summary>
		/// Name of the process/client to simplify analysis of simulation
		/// </summary>
		public string ProcessName { get; set; } = string.Empty;
	}

	/// <summary>
	/// Mapper class to format process timeout response to required format
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
