using Azure;
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
		private readonly IConfiguration _configuration;
		private readonly IConnection _connection;

		private readonly IModel _sendOnlyChannel;

		private readonly IModel _sendAndReplyChannel;
		private readonly EventingBasicConsumer _sendAndReplyConsumer;

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

			_sendAndReplyChannel = _connection.CreateModel();

			_sendAndReplyChannel.QueueDeclare(
				queue: _configuration.GetSection("ConnectionSettings")["SendAndReplySenderQueueName"],
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			_sendAndReplyConsumer = new EventingBasicConsumer(_sendAndReplyChannel);

			_sendAndReplyConsumer.Received += (_, ea) => ResponseHandler(ea);

			_sendAndReplyChannel.BasicConsume(
				consumer: _sendAndReplyConsumer,
				queue: _configuration.GetSection("ConnectionSettings")["SendAndReplySenderQueueName"],
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
			var props = _sendOnlyChannel.CreateBasicProperties();
			var correlationId = Guid.NewGuid().ToString();

			props.Type = MessageType.RectangularPrismRequest.GetDescription();
			props.CorrelationId = correlationId;
			props.ReplyTo = _configuration.GetSection("ConnectionSettings")["SendAndReplySenderQueueName"];

			await Task.Run(() =>
			{
				_sendAndReplyChannel.BasicPublish(
					exchange: "",
					routingKey: "nativesendandreplyreceiver",
					basicProperties: props,
					body: rectangularPrismRequest.ToRabbitMQMessage()
				);
			});
		}

		public async Task SendAndReplyProcessTimeout(ProcessTimeoutRequest processTimeoutRequest)
		{
			var props = _sendOnlyChannel.CreateBasicProperties();
			var correlationId = Guid.NewGuid().ToString();

			props.Type = MessageType.ProcessTimeoutRequest.GetDescription();
			props.CorrelationId = correlationId;
			props.ReplyTo = _configuration.GetSection("ConnectionSettings")["SendAndReplySenderQueueName"];

			await Task.Run(() =>
			{
				_sendAndReplyChannel.BasicPublish(
					exchange: "",
					routingKey: "nativesendandreplyreceiver",
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
				_sendAndReplyChannel.Dispose();
				_connection.Dispose();
			});
		}

		private void ResponseHandler(BasicDeliverEventArgs arguments)
		{
			var body = Encoding.UTF8.GetString(arguments.Body.ToArray());

			if (arguments.BasicProperties.Type.Equals(MessageType.RectangularPrismResponse.GetDescription()))
			{
				var response = JsonSerializer.Deserialize<RectangularPrismResponse>(body);

				if (response != null)
					ConsoleUtils.WriteLineColor(response.ToString(), ConsoleColor.Green);
				else
					ConsoleUtils.WriteLineColor("No response found!", ConsoleColor.Red);
			}
			else if (arguments.BasicProperties.Type.Equals(MessageType.ProcessTimeoutResponse.GetDescription()))
			{
				var response = JsonSerializer.Deserialize<ProcessTimeoutResponse>(body);
				ConsoleUtils.WriteLineColor($"Received process timeout response: {response.ProcessName}", ConsoleColor.Green);
			}
		}
	}
}
