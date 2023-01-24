using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Services.Contracts;
using Services.Models;
using System.Text;
using Utils;

namespace Services.Services
{
	public class RabbitMQSenderService : ISenderService
	{
		private readonly IConnection _connection;
		private readonly IModel _channel;

		private readonly IModel _replyChannel;
		private readonly EventingBasicConsumer _consumer;
		private readonly IBasicProperties _props;
		private readonly Guid _correlationId;

		public RabbitMQSenderService(IConfiguration configuration)
		{
			var connectionFactory = new ConnectionFactory
			{
				HostName = configuration.GetSection("ConnectionSettings")["HostName"]
			};

			_connection = connectionFactory.CreateConnection();
			_channel = _connection.CreateModel();

			_channel.QueueDeclare(
				queue: configuration.GetSection("ConnectionSettings")["QueueName"],
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			_replyChannel = _connection.CreateModel();

			_replyChannel.QueueDeclare(
				queue: configuration.GetSection("ConnectionSettings")["ReplySenderQueueName"],
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			_consumer = new EventingBasicConsumer(_replyChannel);
			_props = _replyChannel.CreateBasicProperties();
			_correlationId = Guid.NewGuid();
			_props.CorrelationId = _correlationId.ToString();

			_props.ReplyTo = configuration.GetSection("ConnectionSettings")["ReplySenderQueueName"];

			_consumer.Received += (model, ea) =>
			{
				var body = ea.Body.ToArray();
				var response = Encoding.UTF8.GetString(body);
				ConsoleUtils.WriteLineColor($"Reply from simple/advanced message: {response}", ConsoleColor.Green);
			};

			_replyChannel.BasicConsume(
				consumer: _consumer,
				queue: configuration.GetSection("ConnectionSettings")["ReplySenderQueueName"],
				autoAck: true
			);
		}

		public async Task SendSimpleMessage(SimpleMessage simpleMessage)
		{
			await Task.Run(() =>
			{
				_channel.BasicPublish(
					exchange: "",
					routingKey: "nativereceiver",
					basicProperties: null,
					body: simpleMessage.ToRabbitMQMessage()
				);
			});
		}

		public async Task SendAdvancedMessage(AdvancedMessage advancedMessage)
		{
			await Task.Run(() =>
			{
				_channel.BasicPublish(
					exchange: "",
					routingKey: "nativereceiver",
					basicProperties: null,
					body: advancedMessage.ToRabbitMQMessage()
				);
			});
		}

		public async Task SendAndReplySimpleMessage(SimpleMessage simpleMessage)
		{
			await Task.Run(() =>
			{
				_channel.BasicPublish(
					exchange: "",
					routingKey: "nativereplyreceiver",
					basicProperties: _props,
					body: simpleMessage.ToRabbitMQMessage()
				);
			});
		}

		public async Task SendAndReplyAdvancedMessage(AdvancedMessage advancedMessage)
		{
			await Task.Run(() =>
			{
				_channel.BasicPublish(
					exchange: "",
					routingKey: "nativereplyreceiver",
					basicProperties: _props,
					body: advancedMessage.ToRabbitMQMessage()
				);
			});
		}

		public async Task FinishJob()
		{
			await Task.Run(() =>
			{
				_channel.Dispose();
				_replyChannel.Dispose();
				_connection.Dispose();
			});
		}
	}
}
