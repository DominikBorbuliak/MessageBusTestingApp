using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Services.Contracts;
using System.Text;
using Utils;

namespace Services.Services
{
	public class RabbitMQSender : ISenderService
	{
		private readonly IConnection _connection;
		private readonly IModel _channel;

		public RabbitMQSender(IConfiguration configuration)
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
		}

		public async Task Run()
		{
			string? message;

			do
			{
				Console.WriteLine("Press enter to exit application or type text of the message!");
				message = Console.ReadLine();

				if (!string.IsNullOrEmpty(message))
				{
					_channel.BasicPublish(
						exchange: "",
						routingKey: "nativereceiver",
						basicProperties: null,
						body: Encoding.UTF8.GetBytes(message)
					);
					ConsoleUtils.WriteLineColor("Message was successfully send to queue!\n", ConsoleColor.Green);
				}
				else
				{
					ConsoleUtils.WriteLineColor("Application was successfully closed!", ConsoleColor.Green);
				}

			} while (!string.IsNullOrEmpty(message));

			_channel.Dispose();
			_connection.Dispose();
		}
	}
}
