using Azure.Messaging.ServiceBus;
using Services.Models;
using System.Text.Json;
using System.Text;

namespace Services.Mappers
{
	/// <summary>
	/// Class used to map rectangular prism response
	/// </summary>
	public static class RectangularPrismResponseMapper
	{
		/// <summary>
		/// Formats RectangularPrismResponse to ServiceBusMessage
		/// </summary>
		/// <param name="rectangularPrismResponse"></param>
		/// <param name="sessionId"></param>
		/// <returns></returns>
		public static ServiceBusMessage ToServiceBusMessage(this RectangularPrismResponse rectangularPrismResponse, string sessionId) => new(JsonSerializer.Serialize(rectangularPrismResponse))
		{
			SessionId = sessionId
		};


		/// <summary>
		/// Formats RectangularPrismResponse to RabbitMQ message
		/// </summary>
		/// <param name="rectangularPrismResponse"></param>
		/// <returns></returns>
		public static byte[] ToRabbitMQMessage(this RectangularPrismResponse rectangularPrismResponse) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(rectangularPrismResponse));
	}
}
