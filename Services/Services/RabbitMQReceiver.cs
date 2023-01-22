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
	public class RabbitMQReceiver : IReceiverService
	{
		private readonly IConnection _connection;
		private readonly IModel _channel;
		private readonly EventingBasicConsumer _consumer;

		public RabbitMQReceiver(IConfiguration configuration)
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
			});
		}

		public async Task FinishJob()
		{
			await Task.Run(() =>
			{
				_connection.Close();

				_channel.Dispose();
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
	}
}
