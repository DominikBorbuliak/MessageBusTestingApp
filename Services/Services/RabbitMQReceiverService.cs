﻿using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Services.Contracts;
using Services.Handlers;
using Services.Mappers;
using Services.Models;
using System.Text;
using System.Text.Json;
using Utils;

namespace Services.Services
{
	public class RabbitMQReceiverService : IReceiverService
	{
		private readonly IConfiguration _configuration;
		private readonly IConnection _connection;

		private readonly IModel _sendOnlyChannel;
		private readonly AsyncEventingBasicConsumer _sendOnlyConsumer;

		private readonly IModel _sendAndReplyNoWaitChannel;
		private readonly AsyncEventingBasicConsumer _sendAndReplyNoWaitConsumer;

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
			_configuration = configuration;

			_deliveryCounts = new Dictionary<string, int>();

			var connectionFactory = new ConnectionFactory
			{
				HostName = _configuration.GetSection("ConnectionSettings")["HostName"],
				DispatchConsumersAsync = true,
				ConsumerDispatchConcurrency = 3
			};

			_connection = connectionFactory.CreateConnection();

			_sendOnlyChannel = _connection.CreateModel();
			_sendAndReplyNoWaitChannel = _connection.CreateModel();

			_sendOnlyChannel.QueueDeclare(
				queue: _configuration.GetSection("ConnectionSettings")["SendOnlyReceiverQueueName"],
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			_sendAndReplyNoWaitChannel.QueueDeclare(
				queue: _configuration.GetSection("ConnectionSettings")["SendAndReplyNoWaitReceiverQueueName"],
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			_sendAndReplyNoWaitChannel.QueueDeclare(
				queue: _configuration.GetSection("ConnectionSettings")["SendAndReplyNoWaitSenderQueueName"],
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			_sendOnlyConsumer = new AsyncEventingBasicConsumer(_sendOnlyChannel);

			_sendAndReplyNoWaitChannel.BasicQos(0, 3, false);
			_sendAndReplyNoWaitConsumer = new AsyncEventingBasicConsumer(_sendAndReplyNoWaitChannel);
		}

		public async Task StartJob()
		{
			await Task.Run(() =>
			{
				_sendOnlyConsumer.Received += (_, eventArguments) => MessageHandler(eventArguments);
				_sendOnlyChannel.BasicConsume(
					queue: _configuration.GetSection("ConnectionSettings")["SendOnlyReceiverQueueName"],
					autoAck: false,
					consumer: _sendOnlyConsumer
				);

				_sendAndReplyNoWaitConsumer.Received += (_, eventArguments) => RequestHandler(eventArguments);
				_sendAndReplyNoWaitChannel.BasicConsume(
					queue: _configuration.GetSection("ConnectionSettings")["SendAndReplyNoWaitReceiverQueueName"],
					autoAck: false,
					consumer: _sendAndReplyNoWaitConsumer
				);
			});
		}

		public async Task FinishJob()
		{
			await Task.Run(() =>
			{
				_connection.Close();

				_sendOnlyChannel.Dispose();
				_sendAndReplyNoWaitChannel.Dispose();

				_connection.Dispose();
			});
		}

		/// <summary>
		/// Handler method used for simple, advanced and exception message
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		private async Task MessageHandler(BasicDeliverEventArgs arguments)
		{
			await Task.Run(() =>
			{
				var body = Encoding.UTF8.GetString(arguments.Body.ToArray());

				var deserializedCorrectly = false;

				if (arguments.BasicProperties.Type.Equals(MessageType.SimpleMessage.GetDescription()))
				{
					ConsoleUtils.WriteLineColor($"Simple messsage received: {body}", ConsoleColor.Green);
					deserializedCorrectly = true;
				}
				else if (arguments.BasicProperties.Type.Equals(MessageType.AdvancedMessage.GetDescription()))
				{
					var advancedMessage = JsonSerializer.Deserialize<AdvancedMessage>(body);
					deserializedCorrectly = AdvancedMessageHandler.Handle(advancedMessage);
				}
				else if (arguments.BasicProperties.Type.Equals(MessageType.ExceptionMessage.GetDescription()))
				{
					try
					{
						var exceptionMessage = JsonSerializer.Deserialize<ExceptionMessage>(body);

						if (!_deliveryCounts.ContainsKey(arguments.BasicProperties.MessageId))
							_deliveryCounts.Add(arguments.BasicProperties.MessageId, 1);
						else
							_deliveryCounts[arguments.BasicProperties.MessageId] += 1;

						var deliveryCount = _deliveryCounts[arguments.BasicProperties.MessageId];

						if (_maxNumberOfDeliveryCounts < deliveryCount)
							return;

						deserializedCorrectly = ExceptionMessageHandler.Handle(exceptionMessage, deliveryCount);
					}
					catch (Exception exception)
					{
						ConsoleUtils.WriteLineColor($"Exception occured: {exception.Message}", ConsoleColor.Red);

						// RabbitMQ does not have built in error handling so we need to use nack to reque
						_sendOnlyChannel.BasicNack(arguments.DeliveryTag, false, true);
						return;
					}
				}

				// Ack message only if deserialization was correct
				if (deserializedCorrectly)
					_sendOnlyChannel.BasicAck(
						deliveryTag: arguments.DeliveryTag,
						multiple: false
					);
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

			if (arguments.BasicProperties.Type.Equals(MessageType.RectangularPrismNoWaitRequest.GetDescription()))
			{
				try
				{
					var rectangularPrismRequest = JsonSerializer.Deserialize<RectangularPrismRequest>(body);

					if (!_deliveryCounts.ContainsKey(arguments.BasicProperties.MessageId))
						_deliveryCounts.Add(arguments.BasicProperties.MessageId, 1);
					else
						_deliveryCounts[arguments.BasicProperties.MessageId] += 1;

					var deliveryCount = _deliveryCounts[arguments.BasicProperties.MessageId];

					// Simulate simple error handling in No Wait
					if (_maxNumberOfDeliveryCounts < deliveryCount)
					{
						var exceptionResponseProps = _sendAndReplyNoWaitChannel.CreateBasicProperties();
						exceptionResponseProps.Type = MessageType.ExceptionResponse.GetDescription();

						_sendAndReplyNoWaitChannel.BasicPublish(
							exchange: "",
							routingKey: arguments.BasicProperties.ReplyTo,
							mandatory: false,
							basicProperties: exceptionResponseProps,
							body: new ExceptionResponse { Text = "No response found for: RectangularPrismResponse!" }.ToRabbitMQMessage()
						);

						_sendAndReplyNoWaitChannel.BasicAck(
							deliveryTag: arguments.DeliveryTag,
							multiple: false
						);

						return;
					}

					var rectangularPrismResponse = RectangularPrismRequestHandler.HandleAndGenerateResponse(rectangularPrismRequest, deliveryCount);

					// Publish response and ack request only if deserialization was correct
					if (rectangularPrismResponse == null)
						return;

					var props = _sendAndReplyNoWaitChannel.CreateBasicProperties();
					props.Type = MessageType.RectangularPrismResponse.GetDescription();

					_sendAndReplyNoWaitChannel.BasicPublish(
						exchange: "",
						routingKey: arguments.BasicProperties.ReplyTo,
						mandatory: false,
						basicProperties: props,
						body: rectangularPrismResponse.ToRabbitMQMessage()
					);
				}
				catch (Exception exception)
				{
					ConsoleUtils.WriteLineColor($"Exception occured: {exception.Message}", ConsoleColor.Red);

					// RabbitMQ does not have built in error handling so we need to use nack to reque
					_sendAndReplyNoWaitChannel.BasicNack(arguments.DeliveryTag, false, true);
					return;
				}
			}
			else if (arguments.BasicProperties.Type.Equals(MessageType.ProcessTimeoutNoWaitRequest.GetDescription()))
			{
				var processTimeoutRequest = JsonSerializer.Deserialize<ProcessTimeoutRequest>(body);

				var processTimeoutResponse = await ProcessTimeoutRequestHandler.HandleAndGenerateResponse(processTimeoutRequest);

				// Publish response and ack request only if deserialization was correct
				if (processTimeoutResponse == null)
					return;

				var props = _sendAndReplyNoWaitChannel.CreateBasicProperties();
				props.Type = MessageType.ProcessTimeoutResponse.GetDescription();

				_sendAndReplyNoWaitChannel.BasicPublish(
					exchange: "",
					routingKey: arguments.BasicProperties.ReplyTo,
					mandatory: false,
					basicProperties: props,
					body: processTimeoutResponse.ToRabbitMQMessage()
				);
			}

			_sendAndReplyNoWaitChannel.BasicAck(
				deliveryTag: arguments.DeliveryTag,
				multiple: false
			);
		}
	}
}
