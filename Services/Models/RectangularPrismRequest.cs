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
		public string ExceptionText { get; set; } = string.Empty;
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
	/// Mapper class to format rectangular prism request to required format
	/// </summary>
	public static class RectangularPrismRequestMapper
	{
		public static ServiceBusMessage ToServiceBusMessage(this RectangularPrismRequest rectangularPrismRequest) => new ServiceBusMessage(JsonSerializer.Serialize(rectangularPrismRequest))
		{
			Subject = MessageType.RectangularPrismRequest.GetDescription()
		};

		public static byte[] ToRabbitMQMessage(this RectangularPrismRequest rectangularPrismRequest) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(rectangularPrismRequest));
	}
}
