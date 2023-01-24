using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Models;
using System.Text.Json;
using Utils;

namespace Services.Services
{
	public class AzureServiceBusSenderService : ISenderService
	{
		private readonly ServiceBusClient _serviceBusClient;

		private readonly ServiceBusSender _sendOnlyServiceBusSender;

		private readonly ServiceBusSender _sendAndReplyServiceBusSender;
		private readonly ServiceBusSessionReceiver _sendAndReplyServiceBusReceiver;
		private readonly Guid _sendAndReplySessionId;

		public AzureServiceBusSenderService(IConfiguration configuration)
		{
			_serviceBusClient = new ServiceBusClient(configuration.GetConnectionString("AzureServiceBus"), new ServiceBusClientOptions()
			{
				TransportType = ServiceBusTransportType.AmqpWebSockets
			});

			_sendAndReplySessionId = Guid.NewGuid();

			_sendOnlyServiceBusSender = _serviceBusClient.CreateSender(configuration.GetSection("ConnectionSettings")["SendOnlyReceiverQueueName"]);
			_sendAndReplyServiceBusSender = _serviceBusClient.CreateSender(configuration.GetSection("ConnectionSettings")["SendAndReplyReceiverQueueName"]);
			_sendAndReplyServiceBusReceiver = _serviceBusClient.AcceptSessionAsync(configuration.GetSection("ConnectionSettings")["SendAndReplySenderQueueName"], _sendAndReplySessionId.ToString()).Result;
		}

		public async Task SendSimpleMessage(SimpleMessage simpleMessage)
		{
			await _sendOnlyServiceBusSender.SendMessageAsync(simpleMessage.ToServiceBusMessage());
		}

		public async Task SendAdvancedMessage(AdvancedMessage advancedMessage)
		{
			await _sendOnlyServiceBusSender.SendMessageAsync(advancedMessage.ToServiceBusMessage());
		}

		public async Task SendAndReplyRectangularPrism(RectangularPrismRequest rectangularPrismRequest)
		{
			var serviceBusMessage = rectangularPrismRequest.ToServiceBusMessage();
			serviceBusMessage.SessionId = _sendAndReplySessionId.ToString();

			await _sendAndReplyServiceBusSender.SendMessageAsync(serviceBusMessage);

			var responseMessage = await _sendAndReplyServiceBusReceiver.ReceiveMessageAsync();
			var response = JsonSerializer.Deserialize<RectangularPrismResponse>(responseMessage.Body);

			if (response != null)
				ConsoleUtils.WriteLineColor(response.ToString(), ConsoleColor.Green);
			else
				ConsoleUtils.WriteLineColor("No response found!", ConsoleColor.Red);
		}

		public async Task FinishJob()
		{
			await _sendOnlyServiceBusSender.DisposeAsync();
			await _sendAndReplyServiceBusSender.DisposeAsync();
			await _sendAndReplyServiceBusReceiver.DisposeAsync();
			await _serviceBusClient.DisposeAsync();
		}
	}
}
