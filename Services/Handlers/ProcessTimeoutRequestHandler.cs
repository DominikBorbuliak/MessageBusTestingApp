using Services.Models;
using Utils;

namespace Services.Handlers
{
	/// <summary>
	/// Class used to handle process timeout request
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
