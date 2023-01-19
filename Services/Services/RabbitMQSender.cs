using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Services.Contracts;
using Services.Models;
using System.Text;

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

		public async Task SendMessageAsync(Message message)
		{
			await Task.Run(() =>
			{
				_channel.BasicPublish(
					exchange: "",
					routingKey: "nativereceiver",
					basicProperties: null,
					body: Encoding.UTF8.GetBytes(message.Text)
				);
			});
		}

		public async Task DisposeAsync()
		{
			await Task.Run(() =>
			{
				_channel.Dispose();
				_connection.Dispose();
			});
		}
	}
}
