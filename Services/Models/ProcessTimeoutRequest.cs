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
		/// <summary>
		/// Formats ProcessTimeoutRequest to ServiceBusMessage
		/// </summary>
		/// <param name="processTimeoutRequest"></param>
		/// <returns></returns>
		public static ServiceBusMessage ToServiceBusMessage(this ProcessTimeoutRequest processTimeoutRequest) => new(JsonSerializer.Serialize(processTimeoutRequest))
		{
			Subject = MessageType.ProcessTimeoutRequest.GetDescription()
		};

		/// <summary>
		/// Formats ProcessTimeoutRequest to RabbitMQ message
		/// </summary>
		/// <param name="processTimeoutRequest"></param>
		/// <returns></returns>
		public static byte[] ToRabbitMQMessage(this ProcessTimeoutRequest processTimeoutRequest) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(processTimeoutRequest));
	}

	/// <summary>
	/// Handler class to handle and generate process timeout response
	/// </summary>
	public static class ProcessTimeoutRequestHandler
	{
		/// <summary>
		/// Handles process timeout request and generates response
		/// </summary>
		/// <param name="processTimeoutRequest">Request to handle</param>
		/// <returns></returns>
		public static async Task<ProcessTimeoutResponse?> HandleAndGenerateResponse(ProcessTimeoutRequest? processTimeoutRequest)
		{
			if (processTimeoutRequest == null)
			{
				ConsoleUtils.WriteLineColor("ProcessTimeoutRequest could not be deserialized correctly!", ConsoleColor.Red);
				return null;
			}

			ConsoleUtils.WriteLineColor($"Received process timeout request: {processTimeoutRequest.ProcessName}. Waiting for: {processTimeoutRequest.MillisecondsTimeout}ms", ConsoleColor.Green);
			await Task.Delay(processTimeoutRequest.MillisecondsTimeout);
			ConsoleUtils.WriteLineColor($"Sending process timeout response: {processTimeoutRequest.ProcessName}", ConsoleColor.Green);

			return new ProcessTimeoutResponse
			{
				ProcessName = processTimeoutRequest.ProcessName
			};
		}
	}
}
