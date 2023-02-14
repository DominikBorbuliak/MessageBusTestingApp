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

		/// <summary>
		/// BasicDeliverEventArgs does not include delivery count property so we replace it with this property
		/// </summary>
		private readonly IDictionary<string, int> _deliveryCounts;

		/// <summary>
		/// Simulate simple error handling logic with maximum number of delivery attempts
		/// </summary>
		private readonly int _maxNumberOfDeliveryCounts = 10;

		public RabbitMQReceiverService(IConfiguration configuration)
		{
			_deliveryCounts = new Dictionary<string, int>();

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
					autoAck: false,
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

					_sendOnlyChannel.BasicAck(
						deliveryTag: arguments.DeliveryTag,
						multiple: false
					);
				}
				else if (arguments.BasicProperties.Type.Equals(MessageType.AdvancedMessage.GetDescription()))
				{
					var advancedMessage = JsonSerializer.Deserialize<AdvancedMessage>(body);

					if (!AdvancedMessageHandler.Handle(advancedMessage))
						return;

					_sendOnlyChannel.BasicAck(
						deliveryTag: arguments.DeliveryTag,
						multiple: false
					);
				}
				else if (arguments.BasicProperties.Type.Equals(MessageType.ExceptionMessage.GetDescription()))
				{
					try
					{
						var exceptionMessage = JsonSerializer.Deserialize<ExceptionMessage>(body);

						if (!_deliveryCounts.ContainsKey(arguments.BasicProperties.MessageId))
							_deliveryCounts.Add(arguments.BasicProperties.MessageId, 1);

						var deliveryCount = _deliveryCounts[arguments.BasicProperties.MessageId];
						_deliveryCounts[arguments.BasicProperties.MessageId] += 1;

						if (_maxNumberOfDeliveryCounts <= deliveryCount)
							return;

						if (!ExceptionMessageHandler.Handle(exceptionMessage, deliveryCount))
							return;

						_sendOnlyChannel.BasicAck(
							deliveryTag: arguments.DeliveryTag,
							multiple: false
						);
					}
					catch (Exception exception)
					{
						ConsoleUtils.WriteLineColor($"Exception occured: {exception.Message}", ConsoleColor.Red);
						_sendOnlyChannel.BasicNack(arguments.DeliveryTag, false, true);
					}
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
				try
				{
					var rectangularPrismRequest = JsonSerializer.Deserialize<RectangularPrismRequest>(body);

					if (!_deliveryCounts.ContainsKey(arguments.BasicProperties.MessageId))
						_deliveryCounts.Add(arguments.BasicProperties.MessageId, 1);

					var deliveryCount = _deliveryCounts[arguments.BasicProperties.MessageId];
					_deliveryCounts[arguments.BasicProperties.MessageId] += 1;

					if (_maxNumberOfDeliveryCounts <= deliveryCount)
					{
						var exceptionResponseProps = _sendAndReplyChannel.CreateBasicProperties();
						exceptionResponseProps.Type = MessageType.ExceptionResponse.GetDescription();

						_sendAndReplyChannel.BasicPublish(
							exchange: "",
							routingKey: arguments.BasicProperties.ReplyTo,
							mandatory: false,
							basicProperties: exceptionResponseProps,
							body: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new ExceptionResponse { Text = "No response found for: RectangularPrismResponse!" }))
						);

						_sendAndReplyChannel.BasicAck(
							deliveryTag: arguments.DeliveryTag,
							multiple: false
						);

						return;
					}

					var rectangularPrismResponse = RectangularPrismRequestHandler.HandleAndGenerateResponse(rectangularPrismRequest, deliveryCount);
					if (rectangularPrismResponse == null)
						return;

					var props = _sendAndReplyChannel.CreateBasicProperties();
					props.Type = MessageType.RectangularPrismResponse.GetDescription();

					_sendAndReplyChannel.BasicPublish(
						exchange: "",
						routingKey: arguments.BasicProperties.ReplyTo,
						mandatory: false,
						basicProperties: props,
						body: rectangularPrismResponse.ToRabbitMQMessage()
					);

					_sendAndReplyChannel.BasicAck(
						deliveryTag: arguments.DeliveryTag,
						multiple: false
					);
				}
				catch (Exception exception)
				{
					ConsoleUtils.WriteLineColor($"Exception occured: {exception.Message}", ConsoleColor.Red);
					_sendAndReplyChannel.BasicNack(arguments.DeliveryTag, false, true);
				}
			}
			else if (arguments.BasicProperties.Type.Equals(MessageType.ProcessTimeoutRequest.GetDescription()))
			{
				var processTimeoutRequest = JsonSerializer.Deserialize<ProcessTimeoutRequest>(body);

				var processTimeoutResponse = await ProcessTimeoutRequestHandler.HandleAndGenerateResponse(processTimeoutRequest);
				if (processTimeoutResponse == null)
					return;

				var props = _sendAndReplyChannel.CreateBasicProperties();
				props.Type = MessageType.ProcessTimeoutResponse.GetDescription();

				_sendAndReplyChannel.BasicPublish(
					exchange: "",
					routingKey: arguments.BasicProperties.ReplyTo,
					mandatory: false,
					basicProperties: props,
					body: processTimeoutResponse.ToRabbitMQMessage()
				);

				_sendAndReplyChannel.BasicAck(
					deliveryTag: arguments.DeliveryTag,
					multiple: false
				);
			}
		}
	}
}
