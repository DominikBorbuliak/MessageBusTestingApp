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
		public static ServiceBusMessage ToServiceBusMessage(this ProcessTimeoutResponse processTimeoutResponse) => new ServiceBusMessage(JsonSerializer.Serialize(processTimeoutResponse));

		public static byte[] ToRabbitMQMessage(this ProcessTimeoutResponse processTimeoutResponse) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(processTimeoutResponse));
	}
}
