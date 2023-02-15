using Services.Models;
using Utils;

namespace Services.Handlers
{
	/// <summary>
	/// Class used to handle advanced message
	/// </summary>
	public static class AdvancedMessageHandler
	{
		/// <summary>
		/// Handles advanced message
		/// </summary>
		/// <param name="advancedMessage">Message to handle</param>
		/// <returns></returns>
		public static bool Handle(AdvancedMessage? advancedMessage)
		{
			if (advancedMessage == null)
			{
				ConsoleUtils.WriteLineColor("AdvancedMessage could not be deserialized correctly!", ConsoleColor.Red);
				return false;
			}

			ConsoleUtils.WriteLineColor($"Advanced messsage received:\n{advancedMessage}", ConsoleColor.Green);
			return true;
		}
	}
}
