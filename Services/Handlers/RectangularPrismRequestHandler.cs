using Services.Models;
using Utils;

namespace Services.Handlers
{
	/// <summary>
	/// Class used to handle rectangular prism request
	/// </summary>
	public static class RectangularPrismRequestHandler
	{
		/// <summary>
		/// Handles rectangular prism requests and generates response
		/// Throws exception if simulating error
		/// </summary>
		/// <param name="rectangularPrismRequest">Request to handle</param>
		/// <param name="deliveryCount">Current delivery attempt</param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static RectangularPrismResponse? HandleAndGenerateResponse(RectangularPrismRequest? rectangularPrismRequest, int deliveryCount)
		{
			if (rectangularPrismRequest == null)
			{
				ConsoleUtils.WriteLineColor("RectangularPrismRequest could not be deserialized correctly!", ConsoleColor.Red);
				return null;
			}

			if (rectangularPrismRequest.SucceedOn <= 0 || deliveryCount < rectangularPrismRequest.SucceedOn)
			{
				ConsoleUtils.WriteLineColor($"Throwing exception with text: {rectangularPrismRequest.ExceptionText}", ConsoleColor.Yellow);
				throw new Exception(rectangularPrismRequest.ExceptionText);
			}

			ConsoleUtils.WriteLineColor($"Rectangular prism request received:\n{rectangularPrismRequest}", ConsoleColor.Green);

			var rectangularPrismResponse = new RectangularPrismResponse
			{
				SurfaceArea = 2 * (rectangularPrismRequest.EdgeA * rectangularPrismRequest.EdgeB + rectangularPrismRequest.EdgeA * rectangularPrismRequest.EdgeC + rectangularPrismRequest.EdgeB * rectangularPrismRequest.EdgeC),
				Volume = rectangularPrismRequest.EdgeA * rectangularPrismRequest.EdgeB * rectangularPrismRequest.EdgeC
			};

			ConsoleUtils.WriteLineColor("Sending rectangular prism response", ConsoleColor.Green);

			return rectangularPrismResponse;
		}
	}
}
