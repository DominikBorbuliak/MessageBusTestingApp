using Microsoft.Extensions.Configuration;
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
	public class RabbitMQSenderService : ISenderService
	{
		private readonly IConfiguration _configuration;
		private readonly IConnection _connection;

		private readonly IModel _sendOnlyChannel;

		private readonly IModel _sendAndReplyNoWaitChannel;
		private readonly EventingBasicConsumer _sendAndReplyNoWaitConsumer;

		public RabbitMQSenderService(IConfiguration configuration)
		{
			_configuration = configuration;

			var connectionFactory = new ConnectionFactory
			{
				HostName = _configuration.GetSection("ConnectionSettings")["HostName"]
			};

			_connection = connectionFactory.CreateConnection();
			_sendOnlyChannel = _connection.CreateModel();

			_sendOnlyChannel.QueueDeclare(
				queue: _configuration.GetSection("ConnectionSettings")["SendOnlyReceiverQueueName"],
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			_sendAndReplyNoWaitChannel = _connection.CreateModel();

			_sendAndReplyNoWaitChannel.QueueDeclare(
				queue: _configuration.GetSection("ConnectionSettings")["SendAndReplyNoWaitSenderQueueName"],
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			_sendAndReplyNoWaitConsumer = new EventingBasicConsumer(_sendAndReplyNoWaitChannel);

			_sendAndReplyNoWaitConsumer.Received += (_, ea) => ResponseHandler(ea);

			_sendAndReplyNoWaitChannel.BasicConsume(
				consumer: _sendAndReplyNoWaitConsumer,
				queue: _configuration.GetSection("ConnectionSettings")["SendAndReplyNoWaitSenderQueueName"],
				autoAck: true
			);
		}

		public async Task SendSimpleMessage(SimpleMessage simpleMessage)
		{
			await Task.Run(() =>
			{
				var props = _sendOnlyChannel.CreateBasicProperties();
				props.Type = MessageType.SimpleMessage.GetDescription();

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
			await Task.Run(() =>
			{
				var props = _sendOnlyChannel.CreateBasicProperties();
				props.Type = MessageType.AdvancedMessage.GetDescription();

				_sendOnlyChannel.BasicPublish(
					exchange: "",
					routingKey: "nativesendonlyreceiver",
					basicProperties: props,
					body: advancedMessage.ToRabbitMQMessage()
				);
			});
		}

		public async Task SendExceptionMessage(ExceptionMessage exceptionMessage)
		{
			await Task.Run(() =>
			{
				var props = _sendOnlyChannel.CreateBasicProperties();
				props.Type = MessageType.ExceptionMessage.GetDescription();
				props.MessageId = Guid.NewGuid().ToString();

				_sendOnlyChannel.BasicPublish(
					exchange: "",
					routingKey: "nativesendonlyreceiver",
					basicProperties: props,
					body: exceptionMessage.ToRabbitMQMessage()
				);
			});
		}

		public async Task SendAndReplyRectangularPrism(RectangularPrismRequest rectangularPrismRequest, bool wait)
		{
			await Task.Run(() =>
			{
				if (wait)
				{
					ConsoleUtils.WriteLineColor("This feature is not possible in RabbitMQ!", ConsoleColor.Yellow);
					return;
				}

				var props = _sendAndReplyNoWaitChannel.CreateBasicProperties();
				var correlationId = Guid.NewGuid().ToString();

				props.Type = MessageType.RectangularPrismNoWaitRequest.GetDescription();
				props.CorrelationId = correlationId;
				props.ReplyTo = _configuration.GetSection("ConnectionSettings")["SendAndReplyNoWaitSenderQueueName"];
				props.MessageId = Guid.NewGuid().ToString();

				_sendAndReplyNoWaitChannel.BasicPublish(
					exchange: "",
					routingKey: "nativesendandreplynowaitreceiver",
					basicProperties: props,
					body: rectangularPrismRequest.ToRabbitMQMessage()
				);
			});
		}

		public async Task SendAndReplyProcessTimeout(ProcessTimeoutRequest processTimeoutRequest, bool wait)
		{
			await Task.Run(() =>
			{
				if (wait)
				{
					ConsoleUtils.WriteLineColor("This feature is not possible in RabbitMQ!", ConsoleColor.Yellow);
					return;
				}

				var props = _sendAndReplyNoWaitChannel.CreateBasicProperties();
				var correlationId = Guid.NewGuid().ToString();

				props.Type = MessageType.ProcessTimeoutNoWaitRequest.GetDescription();
				props.CorrelationId = correlationId;
				props.ReplyTo = _configuration.GetSection("ConnectionSettings")["SendAndReplyNoWaitSenderQueueName"];

				_sendAndReplyNoWaitChannel.BasicPublish(
					exchange: "",
					routingKey: "nativesendandreplynowaitreceiver",
					basicProperties: props,
					body: processTimeoutRequest.ToRabbitMQMessage()
				);
			});
		}

		public async Task FinishJob()
		{
			await Task.Run(() =>
			{
				_sendOnlyChannel.Dispose();
				_sendAndReplyNoWaitChannel.Dispose();
				_connection.Dispose();
			});
		}

		/// <summary>
		/// Handler method used for rectangular prism, process timeout  and exception responses
		/// </summary>
		/// <param name="arguments"></param>
		private static void ResponseHandler(BasicDeliverEventArgs arguments)
		{
			var body = Encoding.UTF8.GetString(arguments.Body.ToArray());

			if (arguments.BasicProperties.Type.Equals(MessageType.RectangularPrismResponse.GetDescription()))
			{
				var rectangularPrismResponse = JsonSerializer.Deserialize<RectangularPrismResponse>(body);
				RectangularPrismResponseHandler.Handle(rectangularPrismResponse);
			}
			else if (arguments.BasicProperties.Type.Equals(MessageType.ProcessTimeoutResponse.GetDescription()))
			{
				var processTimeoutResponse = JsonSerializer.Deserialize<ProcessTimeoutResponse>(body);
				ProcessTimeoutResponseHandler.Handle(processTimeoutResponse);
			}
			else if (arguments.BasicProperties.Type.Equals(MessageType.ExceptionResponse.GetDescription()))
			{
				var exceptionResponse = JsonSerializer.Deserialize<ExceptionResponse>(body);
				ExceptionResponseHandler.Handle(exceptionResponse);
			}

			// Auto ack enabled for _sendAndReplyNoWaitChannel so no need to ack
		}
	}
}
