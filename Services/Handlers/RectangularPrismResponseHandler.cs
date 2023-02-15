using Services.Models;
using Utils;

namespace Services.Handlers
{
	/// <summary>
	/// Class used to handle rectangular prism responsee
	/// </summary>
	public static class RectangularPrismResponseHandler
	{
		/// <summary>
		/// Handles exception message
		/// </summary>
		/// <param name="rectangularPrismResponse">Message to handle</param>
		/// <returns></returns>
		public static bool Handle(RectangularPrismResponse? rectangularPrismResponse)
		{
			if (rectangularPrismResponse != null)
			{
				ConsoleUtils.WriteLineColor(rectangularPrismResponse.ToString(), ConsoleColor.Green);
				return true;
			}

			ConsoleUtils.WriteLineColor("Deserialization error: RectangularPrismResponse!", ConsoleColor.Red);
			return false;
		}
	}
}
