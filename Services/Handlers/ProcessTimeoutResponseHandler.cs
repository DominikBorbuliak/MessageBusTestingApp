using Services.Models;
using Utils;

namespace Services.Handlers
{
	/// <summary>
	/// Class used to handle process timeout response
	/// </summary>
	public static class ProcessTimeoutResponseHandler
	{
		/// <summary>
		/// Handles process timeout repsonse
		/// </summary>
		/// <param name="processTimeoutResponse">Response to handle</param>
		/// <returns></returns>
		public static bool Handle(ProcessTimeoutResponse? processTimeoutResponse)
		{
			if (processTimeoutResponse != null)
			{
				ConsoleUtils.WriteLineColor($"Received process timeout response: {processTimeoutResponse.ProcessName}", ConsoleColor.Green);
				return true;
			}

			ConsoleUtils.WriteLineColor("No response found for: ProcessTimeoutResponse!", ConsoleColor.Red);
			return false;
		}
	}
}
