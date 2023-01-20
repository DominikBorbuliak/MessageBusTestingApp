using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Services.Models
{
	public class AdvancedMessage
	{
		public string Name { get; set; } = string.Empty;
		public string Surname { get; set; } = string.Empty;
		public int Age { get; set; }
		public string Email { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public AdvancedMessageAddress Address { get; set; } = new AdvancedMessageAddress();
	}

	public class AdvancedMessageAddress
	{
		public string StreetName { get; set; } = string.Empty;
		public int BuildingNumber { get; set; }
		public string City { get; set; } = string.Empty;
		public string PostalCode { get; set; } = string.Empty;
		public string Country { get; set; } = string.Empty;
	}

	public static class AdvancedMessageMapper
	{
		public static ServiceBusMessage ToServiceBusMessage(this AdvancedMessage advancedMessage) => new ServiceBusMessage(JsonSerializer.Serialize(advancedMessage));
	}
}
