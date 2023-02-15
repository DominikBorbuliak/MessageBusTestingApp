using Azure.Messaging.ServiceBus;
using Services.Models;
using System.Text;
using System.Text.Json;
using Utils;

namespace Services.Mappers
{
	/// <summary>
	/// Class used to map rectangular prism request
	/// </summary>
	public static class RectangularPrismRequestMapper
	{
		/// <summary>
		/// Maps RectangularPrismRequest to ServiceBusMessage
		/// </summary>
		/// <param name="rectangularPrismRequest">Rectangular prism request to map</param>
		/// <param name="sessionId">Session id for current rectangular prism request</param>
		/// <param name="wait">Determines whether it is request to which sender must wait</param>
		/// <returns></returns>
		public static ServiceBusMessage ToServiceBusMessage(this RectangularPrismRequest rectangularPrismRequest, string sessionId, bool wait) => new(JsonSerializer.Serialize(rectangularPrismRequest))
		{
			Subject = wait ? MessageType.RectangularPrismWaitRequest.GetDescription() : MessageType.RectangularPrismNoWaitRequest.GetDescription(),
			SessionId = sessionId
		};

		/// <summary>
		/// Maps RectangularPrismRequest to RabbitMQ message
		/// </summary>
		/// <param name="rectangularPrismRequest">Rectangular prism request to map</param>
		/// <returns></returns>
		public static byte[] ToRabbitMQMessage(this RectangularPrismRequest rectangularPrismRequest) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(rectangularPrismRequest));
	}
}
