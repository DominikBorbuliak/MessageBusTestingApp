using Services.Models;
using Utils;

namespace Services.Handlers
{
	/// <summary>
	/// Class used to handle rectangular prism response
	/// </summary>
	public static class RectangularPrismResponseHandler
	{
		/// <summary>
		/// Handles rectangular prism response
		/// </summary>
		/// <param name="rectangularPrismResponse">Response to handle</param>
		/// <returns></returns>
		public static bool Handle(RectangularPrismResponse? rectangularPrismResponse)
		{
			if (rectangularPrismResponse == null)
			{
				ConsoleUtils.WriteLineColor("RectangularPrismResponse could not be deserialized correctly!", ConsoleColor.Red);
				return false;
			}

			ConsoleUtils.WriteLineColor($"Rectangular prism response received:\n{rectangularPrismResponse}", ConsoleColor.Green);
			return true;
		}
	}
}
