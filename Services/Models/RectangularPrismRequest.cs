using Azure.Messaging.ServiceBus;
using System.Text.Json;
using System.Text;

namespace Services.Models
{
	public class RectangularPrismRequest : IMessage
	{
		public double EdgeA { get; set; }
		public double EdgeB { get; set; }
		public double EdgeC { get; set; }

		public override string ToString()
		{
			var stringBuilder = new StringBuilder();

			stringBuilder.AppendLine($"Edge A: {EdgeA}");
			stringBuilder.AppendLine($"Edge B: {EdgeB}");
			stringBuilder.AppendLine($"Edge C: {EdgeC}");

			return stringBuilder.ToString();
		}
	}

	public static class RectangularPrismRequestMapper
	{
		public static ServiceBusMessage ToServiceBusMessage(this RectangularPrismRequest rectangularPrismRequest) => new ServiceBusMessage(JsonSerializer.Serialize(rectangularPrismRequest));
		public static byte[] ToRabbitMQMessage(this RectangularPrismRequest rectangularPrismRequest) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(rectangularPrismRequest));
	}
}
