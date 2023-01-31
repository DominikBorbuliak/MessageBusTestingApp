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
		private readonly AsyncEventingBasicConsumer _sendOnlyConsumer;

		private readonly IModel _sendAndReplyChannel;
		private readonly AsyncEventingBasicConsumer _sendAndReplyConsumer;

		public RabbitMQReceiverService(IConfiguration configuration)
		{
			var connectionFactory = new ConnectionFactory
			{
				HostName = configuration.GetSection("ConnectionSettings")["HostName"],
				DispatchConsumersAsync = true,
				ConsumerDispatchConcurrency = 3
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

			_sendOnlyConsumer = new AsyncEventingBasicConsumer(_sendOnlyChannel);

			_sendAndReplyChannel = _connection.CreateModel();

			_sendAndReplyChannel.QueueDeclare(
				queue: configuration.GetSection("ConnectionSettings")["SendAndReplyReceiverQueueName"],
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			_sendAndReplyChannel.BasicQos(0, 3, false);
			_sendAndReplyConsumer = new AsyncEventingBasicConsumer(_sendAndReplyChannel);
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

				_sendAndReplyConsumer.Received += (_, eventArguments) => RequestHandler(eventArguments);

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

		/// <summary>
		/// Handler method used for simple and advanced messages
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		private async Task MessageHandler(BasicDeliverEventArgs arguments)
		{
			await Task.Run(() =>
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
			});
		}

		/// <summary>
		/// Handler method used for rectangular prism and process timeout requests
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		private async Task RequestHandler(BasicDeliverEventArgs arguments)
		{
			var body = Encoding.UTF8.GetString(arguments.Body.ToArray());

			if (arguments.BasicProperties.Type.Equals(MessageType.RectangularPrismRequest.GetDescription()))
			{
				var rectangularPrismRequest = JsonSerializer.Deserialize<RectangularPrismRequest>(body);

				if (rectangularPrismRequest == null)
				{
					ConsoleUtils.WriteLineColor("No request found for: RectangularPrismRequest!", ConsoleColor.Red);
					return;
				}

				ConsoleUtils.WriteLineColor($"Rectangular prism request received:\n{rectangularPrismRequest}", ConsoleColor.Green);

				var rectangularPrismResponse = new RectangularPrismResponse
				{
					SurfaceArea = 2 * (rectangularPrismRequest.EdgeA * rectangularPrismRequest.EdgeB + rectangularPrismRequest.EdgeA * rectangularPrismRequest.EdgeC + rectangularPrismRequest.EdgeB * rectangularPrismRequest.EdgeC),
					Volume = rectangularPrismRequest.EdgeA * rectangularPrismRequest.EdgeB * rectangularPrismRequest.EdgeC
				};

				ConsoleUtils.WriteLineColor("Sending rectangular prism response", ConsoleColor.Green);

				var props = _sendAndReplyChannel.CreateBasicProperties();
				props.Type = MessageType.RectangularPrismResponse.GetDescription();

				_sendAndReplyChannel.BasicPublish(
					exchange: "",
					routingKey: arguments.BasicProperties.ReplyTo,
					mandatory: false,
					basicProperties: props,
					body: rectangularPrismResponse.ToRabbitMQMessage()
				);
			}
			else if (arguments.BasicProperties.Type.Equals(MessageType.ProcessTimeoutRequest.GetDescription()))
			{
				var processTimeoutRequest = JsonSerializer.Deserialize<ProcessTimeoutRequest>(body);

				if (processTimeoutRequest == null)
				{
					ConsoleUtils.WriteLineColor("No request found for: ProcessTimeoutRequest!", ConsoleColor.Red);
					return;
				}

				ConsoleUtils.WriteLineColor($"Received process timeout request: {processTimeoutRequest.ProcessName}. Waiting for: {processTimeoutRequest.MillisecondsTimeout}ms", ConsoleColor.Green);
				await Task.Delay(processTimeoutRequest.MillisecondsTimeout);
				ConsoleUtils.WriteLineColor($"Sending process timeout response: {processTimeoutRequest.ProcessName}", ConsoleColor.Green);

				var processTimeoutResponse = new ProcessTimeoutResponse
				{
					ProcessName = processTimeoutRequest.ProcessName
				};

				var props = _sendAndReplyChannel.CreateBasicProperties();
				props.Type = MessageType.ProcessTimeoutResponse.GetDescription();

				_sendAndReplyChannel.BasicPublish(
					exchange: "",
					routingKey: arguments.BasicProperties.ReplyTo,
					mandatory: false,
					basicProperties: props,
					body: processTimeoutResponse.ToRabbitMQMessage()
				);
			}

			_sendAndReplyChannel.BasicAck(
				deliveryTag: arguments.DeliveryTag,
				multiple: false
			);
		}
	}
}
