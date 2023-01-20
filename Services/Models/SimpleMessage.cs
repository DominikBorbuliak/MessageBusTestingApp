using Azure.Messaging.ServiceBus;

namespace Services.Models
{
	public class SimpleMessage
	{
		public string Text { get; set; } = string.Empty;
	}

	public static class SimpleMessageMapper
	{
		public static ServiceBusMessage ToServiceBusMessage(this SimpleMessage simpleMessage) => new ServiceBusMessage(simpleMessage.Text);
	}
}
