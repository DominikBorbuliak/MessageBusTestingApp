using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Services.Contracts;
using System.Text;
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

		public async Task Run()
		{
			try
			{
				_consumer.Received += (_, eventArguments) => MessageHandler(eventArguments);

				_channel.BasicConsume(
					queue: "nativereceiver",
					autoAck: true,
					consumer: _consumer
				);

				Console.WriteLine("Press any key to exit application and stop processing!");
				Console.ReadKey();

				_connection.Close();
			}
			finally
			{
				_channel.Dispose();
				_connection.Dispose();
			}
		}

		private void MessageHandler(BasicDeliverEventArgs arguments)
		{
			var body = Encoding.UTF8.GetString(arguments.Body.ToArray());

			ConsoleUtils.WriteLineColor($"Messsage received: {body}", ConsoleColor.Green);
		}
	}
}
