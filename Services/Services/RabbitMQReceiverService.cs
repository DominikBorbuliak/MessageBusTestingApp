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
	public class RabbitMQReceiverService : IReceiverService
	{
		private readonly IConnection _connection;
		private readonly IModel _channel;
		private readonly EventingBasicConsumer _consumer;

		private readonly IModel _replyChannel;
		private readonly EventingBasicConsumer _replyConsumer;

		public RabbitMQReceiverService(IConfiguration configuration)
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

			_consumer = new EventingBasicConsumer(_channel);

			_replyChannel = _connection.CreateModel();

			_replyChannel.QueueDeclare(
				queue: configuration.GetSection("ConnectionSettings")["ReplyReceiverQueueName"],
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			_replyChannel.BasicQos(0, 1, false);
			_replyConsumer = new EventingBasicConsumer(_replyChannel);
		}

		public async Task StartJob()
		{
			await Task.Run(() =>
			{
				_consumer.Received += (_, eventArguments) => MessageHandler(eventArguments);

				_channel.BasicConsume(
					queue: "nativereceiver",
					autoAck: true,
					consumer: _consumer
				);

				_replyConsumer.Received += (_, eventArguments) => ReplyMessageHandler(eventArguments);

				_replyChannel.BasicConsume(
					queue: "nativereplyreceiver",
					autoAck: false,
					consumer: _replyConsumer
				);
			});
		}

		public async Task FinishJob()
		{
			await Task.Run(() =>
			{
				_connection.Close();

				_channel.Dispose();
				_replyChannel.Dispose();

				_connection.Dispose();
			});
		}

		private void MessageHandler(BasicDeliverEventArgs arguments)
		{
			var body = Encoding.UTF8.GetString(arguments.Body.ToArray());

			try
			{
				var advancedMessage = JsonSerializer.Deserialize<AdvancedMessage>(body);
				ConsoleUtils.WriteLineColor($"Advanced messsage received:\n{advancedMessage}", ConsoleColor.Green);
			}
			catch
			{
				ConsoleUtils.WriteLineColor($"Simple messsage received: {body}", ConsoleColor.Green);
			}
		}

		private void ReplyMessageHandler(BasicDeliverEventArgs arguments)
		{
			var body = Encoding.UTF8.GetString(arguments.Body.ToArray());

			try
			{
				var advancedMessage = JsonSerializer.Deserialize<AdvancedMessage>(body);
				ConsoleUtils.WriteLineColor($"Advanced messsage received:\n{advancedMessage}", ConsoleColor.Green);
			}
			catch
			{
				ConsoleUtils.WriteLineColor($"Simple messsage received: {body}", ConsoleColor.Green);
			}

			ConsoleUtils.WriteLineColor("Sending response", ConsoleColor.Green);

			_replyChannel.BasicPublish(
					exchange: "",
					routingKey: arguments.BasicProperties.ReplyTo,
					mandatory: false,
					basicProperties: _replyChannel.CreateBasicProperties(),
					body: Encoding.UTF8.GetBytes("Message was successfuly received")
				);

			_replyChannel.BasicAck(
				deliveryTag: arguments.DeliveryTag,
				multiple: false
			);
		}
	}
}
