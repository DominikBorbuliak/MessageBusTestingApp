using Azure.Messaging.ServiceBus;
using Services.Models;
using System.Text.Json;
using System.Text;
using Utils;

namespace Services.Mappers
{
	/// <summary>
	/// Class used to map rectangular prism request
	/// </summary>
	public static class RectangularPrismRequestMapper
	{
		/// <summary>
		/// Formats RectangularPrismRequest to ServiceBusMessage
		/// </summary>
		/// <param name="rectangularPrismRequest"></param>
		/// <param name="sessionId"></param>
		/// <returns></returns>
		public static ServiceBusMessage ToServiceBusMessage(this RectangularPrismRequest rectangularPrismRequest, string sessionId) => new(JsonSerializer.Serialize(rectangularPrismRequest))
		{
			Subject = MessageType.RectangularPrismRequest.GetDescription(),
			SessionId = sessionId
		};

		/// <summary>
		/// Formats RectangularPrismRequest to RabbitMQ message
		/// </summary>
		/// <param name="rectangularPrismRequest"></param>
		/// <returns></returns>
		public static byte[] ToRabbitMQMessage(this RectangularPrismRequest rectangularPrismRequest) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(rectangularPrismRequest));
	}
}
