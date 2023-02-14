using Azure.Messaging.ServiceBus;
using System.Text;
using System.Text.Json;

namespace Services.Models
{
	/// <summary>
	/// Model used to simulate request that need to wait for response
	/// </summary>
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

	/// <summary>
	/// Mapper class to format rectangular prism request to required format
	/// </summary>
	public static class RectangularPrismResponseMapper
	{
		/// <summary>
		/// Formats RectangularPrismResponse to ServiceBusMessage
		/// </summary>
		/// <param name="rectangularPrismResponse"></param>
		/// <returns></returns>
		public static ServiceBusMessage ToServiceBusMessage(this RectangularPrismResponse rectangularPrismResponse) => new(JsonSerializer.Serialize(rectangularPrismResponse));

		/// <summary>
		/// Formats RectangularPrismResponse to RabbitMQ message
		/// </summary>
		/// <param name="rectangularPrismResponse"></param>
		/// <returns></returns>
		public static byte[] ToRabbitMQMessage(this RectangularPrismResponse rectangularPrismResponse) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(rectangularPrismResponse));
	}
}
