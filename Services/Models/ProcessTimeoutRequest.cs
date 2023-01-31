using Azure.Messaging.ServiceBus;
using System.Text;
using System.Text.Json;
using Utils;

namespace Services.Models
{
	/// <summary>
	/// Model used to simulate N concurent clients
	/// </summary>
	public class ProcessTimeoutRequest : IMessage
	{
		/// <summary>
		/// Time for which receiver will sleep to simulate workload
		/// </summary>
		public int MillisecondsTimeout { get; set; }

		/// <summary>
		/// Name of the process/client to simplify analysis of simulation
		/// </summary>
		public string ProcessName { get; set; } = string.Empty;
	}

	/// <summary>
	/// Mapper class to format process timeout request to required format
	/// </summary>
	public static class ProcessTimeoutRequestMapper
	{
		public static ServiceBusMessage ToServiceBusMessage(this ProcessTimeoutRequest processTimeoutRequest) => new ServiceBusMessage(JsonSerializer.Serialize(processTimeoutRequest))
		{
			Subject = MessageType.ProcessTimeoutRequest.GetDescription()
		};

		public static byte[] ToRabbitMQMessage(this ProcessTimeoutRequest processTimeoutRequest) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(processTimeoutRequest));
	}
}
