using System.Text;
using Utils;

namespace Services.Models
{
	/// <summary>
	/// Model used to simulate request that need to wait for response
	/// </summary>
	public class RectangularPrismRequest : IMessage
	{
		public double EdgeA { get; set; }

		public double EdgeB { get; set; }

		public double EdgeC { get; set; }

		/// <summary>
		/// Text of the exception that will be thrown in receiver
		/// </summary>
		public string ExceptionText { get; set; } = string.Empty;

		/// <summary>
		/// Number of attempt on which reciever should successfuly process message
		/// 0 or less - never
		/// 1 - first attempt
		/// </summary>
		public int SucceedOn { get; set; }

		public override string ToString()
		{
			var stringBuilder = new StringBuilder();

			stringBuilder.AppendLine($"Edge A: {EdgeA}");
			stringBuilder.AppendLine($"Edge B: {EdgeB}");
			stringBuilder.AppendLine($"Edge C: {EdgeC}");

			return stringBuilder.ToString();
		}
	}

	/// <summary>
	/// Handler class to handle and generate prism response
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
