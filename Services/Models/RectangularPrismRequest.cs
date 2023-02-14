using Azure.Messaging.ServiceBus;
using System.Text;
using System.Text.Json;
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
				ConsoleUtils.WriteLineColor("RectangularPrismRequest not found!", ConsoleColor.Red);
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

	/// <summary>
	/// Mapper class to format rectangular prism request to required format
	/// </summary>
	public static class RectangularPrismRequestMapper
	{
		/// <summary>
		/// Formats RectangularPrismRequest to ServiceBusMessage
		/// </summary>
		/// <param name="rectangularPrismRequest"></param>
		/// <returns></returns>
		public static ServiceBusMessage ToServiceBusMessage(this RectangularPrismRequest rectangularPrismRequest) => new(JsonSerializer.Serialize(rectangularPrismRequest))
		{
			Subject = MessageType.RectangularPrismRequest.GetDescription()
		};

		/// <summary>
		/// Formats RectangularPrismRequest to RabbitMQ message
		/// </summary>
		/// <param name="rectangularPrismRequest"></param>
		/// <returns></returns>
		public static byte[] ToRabbitMQMessage(this RectangularPrismRequest rectangularPrismRequest) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(rectangularPrismRequest));
	}
}
