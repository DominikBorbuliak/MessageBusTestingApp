using Azure.Messaging.ServiceBus;
using Services.Models;
using System.Text;
using System.Text.Json;
using Utils;

namespace Services.Mappers
{
	/// <summary>
	/// Class used to map rectangular prism response
	/// </summary>
	public static class RectangularPrismResponseMapper
	{
		/// <summary>
		/// Maps RectangularPrismResponse to ServiceBusMessage
		/// </summary>
		/// <param name="rectangularPrismResponse">Rectangular prism response to map</param>
		/// <param name="sessionId">Session id for current rectangular prism response</param>
		/// <returns></returns>
		public static ServiceBusMessage ToServiceBusMessage(this RectangularPrismResponse rectangularPrismResponse, string sessionId) => new(JsonSerializer.Serialize(rectangularPrismResponse))
		{
			Subject = MessageType.RectangularPrismResponse.GetDescription(),
			SessionId = sessionId
		};


		/// <summary>
		/// Maps RectangularPrismResponse to RabbitMQ message
		/// </summary>
		/// <param name="rectangularPrismResponse">Rectangular prism response to map</param>
		/// <returns></returns>
		public static byte[] ToRabbitMQMessage(this RectangularPrismResponse rectangularPrismResponse) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(rectangularPrismResponse));
	}
}
