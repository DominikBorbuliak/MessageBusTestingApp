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

		private readonly IModel _sendOnlyChannel;
		private readonly EventingBasicConsumer _sendOnlyConsumer;

		private readonly IModel _sendAndReplyChannel;
		private readonly EventingBasicConsumer _sendAndReplyConsumer;

		public RabbitMQReceiverService(IConfiguration configuration)
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

			_sendOnlyConsumer = new EventingBasicConsumer(_sendOnlyChannel);

			_sendAndReplyChannel = _connection.CreateModel();

			_sendAndReplyChannel.QueueDeclare(
				queue: configuration.GetSection("ConnectionSettings")["SendAndReplyReceiverQueueName"],
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			_sendAndReplyChannel.BasicQos(0, 1, false);
			_sendAndReplyConsumer = new EventingBasicConsumer(_sendAndReplyChannel);
		}

		public async Task StartJob()
		{
			await Task.Run(() =>
			{
				_sendOnlyConsumer.Received += (_, eventArguments) => MessageHandler(eventArguments);

				_sendOnlyChannel.BasicConsume(
					queue: "nativesendonlyreceiver",
					autoAck: true,
					consumer: _sendOnlyConsumer
				);

				_sendAndReplyConsumer.Received += (_, eventArguments) => RectangularPrismRequestHandler(eventArguments);

				_sendAndReplyChannel.BasicConsume(
					queue: "nativesendandreplyreceiver",
					autoAck: false,
					consumer: _sendAndReplyConsumer
				);
			});
		}

		public async Task FinishJob()
		{
			await Task.Run(() =>
			{
				_connection.Close();

				_sendOnlyChannel.Dispose();
				_sendAndReplyChannel.Dispose();

				_connection.Dispose();
			});
		}

		private void MessageHandler(BasicDeliverEventArgs arguments)
		{
			var body = Encoding.UTF8.GetString(arguments.Body.ToArray());

			if (arguments.BasicProperties.Type.Equals(MessageType.SimpleMessage.GetDescription()))
			{
				ConsoleUtils.WriteLineColor($"Simple messsage received: {body}", ConsoleColor.Green);
			}
			else if (arguments.BasicProperties.Type.Equals(MessageType.AdvancedMessage.GetDescription()))
			{
				var advancedMessage = JsonSerializer.Deserialize<AdvancedMessage>(body);
				ConsoleUtils.WriteLineColor($"Advanced messsage received:\n{advancedMessage}", ConsoleColor.Green);
			}
		}

		private void RectangularPrismRequestHandler(BasicDeliverEventArgs arguments)
		{
			var body = Encoding.UTF8.GetString(arguments.Body.ToArray());

			var rectangularPrismRequest = JsonSerializer.Deserialize<RectangularPrismRequest>(body);
			ConsoleUtils.WriteLineColor($"Rectangular prism request received:\n{rectangularPrismRequest}", ConsoleColor.Green);

			var rectangularPrismResponse = new RectangularPrismResponse
			{
				SurfaceArea = 2 * (rectangularPrismRequest.EdgeA * rectangularPrismRequest.EdgeB + rectangularPrismRequest.EdgeA * rectangularPrismRequest.EdgeC + rectangularPrismRequest.EdgeB * rectangularPrismRequest.EdgeC),
				Volume = rectangularPrismRequest.EdgeA * rectangularPrismRequest.EdgeB * rectangularPrismRequest.EdgeC
			};

			ConsoleUtils.WriteLineColor("Sending rectangular prism response", ConsoleColor.Green);

			_sendAndReplyChannel.BasicPublish(
					exchange: "",
					routingKey: arguments.BasicProperties.ReplyTo,
					mandatory: false,
					basicProperties: _sendAndReplyChannel.CreateBasicProperties(),
					body: rectangularPrismResponse.ToRabbitMQMessage()
				);

			_sendAndReplyChannel.BasicAck(
				deliveryTag: arguments.DeliveryTag,
				multiple: false
			);
		}
	}
}
