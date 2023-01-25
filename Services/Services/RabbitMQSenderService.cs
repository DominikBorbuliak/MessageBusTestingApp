using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Services.Contracts;
using Services.Models;
using System.Text;
using System.Text.Json;
using Utils;

namespace Services.Services
{
	public class RabbitMQSenderService : ISenderService
	{
		private readonly IConnection _connection;

		private readonly IModel _sendOnlyChannel;

		private readonly IModel _sendAndReplyChannel;
		private readonly EventingBasicConsumer _sendAndReplyConsumer;
		private readonly IBasicProperties _sendAndReplyProps;
		private readonly Guid _sendAndReplyCorrelationId;

		public RabbitMQSenderService(IConfiguration configuration)
		{
			var connectionFactory = new ConnectionFactory
			{
				HostName = configuration.GetSection("ConnectionSettings")["HostName"]
			};

			_connection = connectionFactory.CreateConnection();
			_sendOnlyChannel = _connection.CreateModel();

			_sendOnlyChannel.QueueDeclare(
				queue: configuration.GetSection("ConnectionSettings")["SendOnlyReceiverQueueName"],
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			_sendAndReplyChannel = _connection.CreateModel();

			_sendAndReplyChannel.QueueDeclare(
				queue: configuration.GetSection("ConnectionSettings")["SendAndReplySenderQueueName"],
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			_sendAndReplyConsumer = new EventingBasicConsumer(_sendAndReplyChannel);
			_sendAndReplyProps = _sendAndReplyChannel.CreateBasicProperties();
			_sendAndReplyCorrelationId = Guid.NewGuid();
			_sendAndReplyProps.CorrelationId = _sendAndReplyCorrelationId.ToString();

			_sendAndReplyProps.ReplyTo = configuration.GetSection("ConnectionSettings")["SendAndReplySenderQueueName"];

			_sendAndReplyConsumer.Received += (_, ea) => RectangularPrismResponseHandler(ea);

			_sendAndReplyChannel.BasicConsume(
				consumer: _sendAndReplyConsumer,
				queue: configuration.GetSection("ConnectionSettings")["SendAndReplySenderQueueName"],
				autoAck: true
			);
		}

		public async Task SendSimpleMessage(SimpleMessage simpleMessage)
		{
			var props = _sendOnlyChannel.CreateBasicProperties();
			props.Type = MessageType.SimpleMessage.GetDescription();

			await Task.Run(() =>
			{
				_sendOnlyChannel.BasicPublish(
					exchange: "",
					routingKey: "nativesendonlyreceiver",
					basicProperties: props,
					body: simpleMessage.ToRabbitMQMessage()
				);
			});
		}

		public async Task SendAdvancedMessage(AdvancedMessage advancedMessage)
		{
			var props = _sendOnlyChannel.CreateBasicProperties();
			props.Type = MessageType.AdvancedMessage.GetDescription();

			await Task.Run(() =>
			{
				_sendOnlyChannel.BasicPublish(
					exchange: "",
					routingKey: "nativesendonlyreceiver",
					basicProperties: props,
					body: advancedMessage.ToRabbitMQMessage()
				);
			});
		}

		public async Task SendAndReplyRectangularPrism(RectangularPrismRequest rectangularPrismRequest)
		{
			await Task.Run(() =>
			{
				_sendAndReplyChannel.BasicPublish(
					exchange: "",
					routingKey: "nativesendandreplyreceiver",
					basicProperties: _sendAndReplyProps,
					body: rectangularPrismRequest.ToRabbitMQMessage()
				);
			});
		}

		public async Task FinishJob()
		{
			await Task.Run(() =>
			{
				_sendOnlyChannel.Dispose();
				_sendAndReplyChannel.Dispose();
				_connection.Dispose();
			});
		}

		private void RectangularPrismResponseHandler(BasicDeliverEventArgs arguments)
		{
			var body = Encoding.UTF8.GetString(arguments.Body.ToArray());

			var response = JsonSerializer.Deserialize<RectangularPrismResponse>(body);

			if (response != null)
				ConsoleUtils.WriteLineColor(response.ToString(), ConsoleColor.Green);
			else
				ConsoleUtils.WriteLineColor("No response found!", ConsoleColor.Red);
		}
	}
}
