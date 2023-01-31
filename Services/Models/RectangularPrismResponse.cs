using Azure.Messaging.ServiceBus;
using System.Text.Json;
using System.Text;
using Utils;

namespace Services.Models
{
	public class RectangularPrismResponse : IMessage
	{
		public double SurfaceArea { get; set; }
		public double Volume { get; set; }

		public override string ToString()
		{
			var stringBuilder = new StringBuilder();

			stringBuilder.AppendLine($"Surface Area: {SurfaceArea}");
			stringBuilder.AppendLine($"Volume: {Volume}");

			return stringBuilder.ToString();
		}
	}

	public static class RectangularPrismResponseMapper
	{
		public static ServiceBusMessage ToServiceBusMessage(this RectangularPrismResponse rectangularPrismResponse) => new ServiceBusMessage(JsonSerializer.Serialize(rectangularPrismResponse));

		public static byte[] ToRabbitMQMessage(this RectangularPrismResponse rectangularPrismResponse) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(rectangularPrismResponse));
	}
}
