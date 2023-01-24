using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Models;
using Utils;

namespace Services.Services
{
	public class AzureServiceBusSenderService : ISenderService
	{
		private readonly ServiceBusClient _serviceBusClient;
		private readonly ServiceBusSender _serviceBusSender;
		private readonly ServiceBusSender _serviceBusSessionSender;
		private readonly ServiceBusSessionReceiver _serviceBusSessionReceiver;
		private readonly Guid _sessionId;

		public AzureServiceBusSenderService(IConfiguration configuration)
		{
			_serviceBusClient = new ServiceBusClient(configuration.GetConnectionString("AzureServiceBus"), new ServiceBusClientOptions()
			{
				TransportType = ServiceBusTransportType.AmqpWebSockets
			});

			_sessionId = Guid.NewGuid();

			_serviceBusSender = _serviceBusClient.CreateSender(configuration.GetSection("ConnectionSettings")["ReceiverQueueName"]);
			_serviceBusSessionSender = _serviceBusClient.CreateSender(configuration.GetSection("ConnectionSettings")["SessionReceiverQueueName"]);
			_serviceBusSessionReceiver = _serviceBusClient.AcceptSessionAsync(configuration.GetSection("ConnectionSettings")["SessionSenderQueueName"], _sessionId.ToString()).Result;
		}

		public async Task SendSimpleMessage(SimpleMessage simpleMessage)
		{
			await _serviceBusSender.SendMessageAsync(simpleMessage.ToServiceBusMessage());
		}

		public async Task SendAdvancedMessage(AdvancedMessage advancedMessage)
		{
			await _serviceBusSender.SendMessageAsync(advancedMessage.ToServiceBusMessage());
		}

		public async Task SendAndReplySimpleMessage(SimpleMessage simpleMessage)
		{
			var serviceBusMessage = simpleMessage.ToServiceBusMessage();
			serviceBusMessage.SessionId = _sessionId.ToString();

			await _serviceBusSessionSender.SendMessageAsync(serviceBusMessage);

			var replyMessage = await _serviceBusSessionReceiver.ReceiveMessageAsync();
			ConsoleUtils.WriteLineColor($"Reply from simple message: {replyMessage.Body}", ConsoleColor.Green);
		}

		public async Task SendAndReplyAdvancedMessage(AdvancedMessage advancedMessage)
		{
			var serviceBusMessage = advancedMessage.ToServiceBusMessage();
			serviceBusMessage.SessionId = _sessionId.ToString();

			await _serviceBusSessionSender.SendMessageAsync(serviceBusMessage);

			var replyMessage = await _serviceBusSessionReceiver.ReceiveMessageAsync();
			ConsoleUtils.WriteLineColor($"Reply from advanced message: {replyMessage.Body}", ConsoleColor.Green);
		}

		public async Task FinishJob()
		{
			await _serviceBusSender.DisposeAsync();
			await _serviceBusSessionSender.DisposeAsync();
			await _serviceBusSessionReceiver.DisposeAsync();
			await _serviceBusClient.DisposeAsync();
		}
	}
}
