using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Services.Contracts;
using Services.Models;
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
			try
			{
				Menu<ActionType> actionMenu = new Menu<ActionType>("Please select action type.", "Use arrow DOWN and UP to navigate through menu.\nPress ENTER to submit.", true);
				ActionType? pickedActionMenuItem;

				while ((pickedActionMenuItem = actionMenu.HandleMenuMovement()) != null)
				{
					Console.Clear();
					Console.CursorVisible = true;

					switch (pickedActionMenuItem)
					{
						case ActionType.SendOnlyOneCustomSimpleMessage:
							await HandleSendOnlyOneCustomSimpleMessage();
							break;
						case ActionType.SendOnlyOneCustomAdvancedMessage:
							await HandleSendOnlyOneCustomAdvancedMessage();
							break;
						case ActionType.SendOnlyNRandomSimpleMessages:
							await HandleSendOnlyNRandomSimpleMessages();
							break;
						case ActionType.SendOnlyNRandomAdvancedMessages:
							await HandleSendOnlyNRandomAdvancedMessages();
							break;
					}

					ConsoleUtils.WriteLineColor("Message was successfully send to queue!", ConsoleColor.Green);
					Thread.Sleep(1000);
				}
			}
			catch
			{
				ConsoleUtils.WriteLineColor($"Error occured. Press anything to exit application.", ConsoleColor.Red);
				Console.ReadKey();
			}
			finally
			{
				_channel.Dispose();
				_connection.Dispose();
			}
		}

		private async Task HandleSendOnlyOneCustomSimpleMessage()
		{
			var simpleMessage = new SimpleMessage
			{
				Text = ConsoleUtils.GetUserTextInput("Please insert text of message:")
			};

			_channel.BasicPublish(
						exchange: "",
						routingKey: "nativereceiver",
						basicProperties: null,
						body: simpleMessage.ToRabbitMQMessage()
					);
		}

		private async Task HandleSendOnlyOneCustomAdvancedMessage()
		{
			var advancedMessage = new AdvancedMessage
			{
				Name = ConsoleUtils.GetUserTextInput("Please insert the name:"),
				Surname = ConsoleUtils.GetUserTextInput("Please insert the surname:"),
				Age = ConsoleUtils.GetUserNumberInput("Please enter the age:"),
				Email = ConsoleUtils.GetUserTextInput("Please enter the email:"),
				Description = ConsoleUtils.GetUserTextInput("Please enter the description:"),
				Address = new AdvancedMessageAddress
				{
					StreetName = ConsoleUtils.GetUserTextInput("Please enter the street name:"),
					BuildingNumber = ConsoleUtils.GetUserNumberInput("Please enter the street number:"),
					City = ConsoleUtils.GetUserTextInput("Please enter the city:"),
					PostalCode = ConsoleUtils.GetUserTextInput("Please enter the postal code:"),
					Country = ConsoleUtils.GetUserTextInput("Please enter the country:")
				}
			};

			_channel.BasicPublish(
						exchange: "",
						routingKey: "nativereceiver",
						basicProperties: null,
						body: advancedMessage.ToRabbitMQMessage()
					);
		}

		private async Task HandleSendOnlyNRandomSimpleMessages()
		{
			var n = ConsoleUtils.GetUserNumberInput("Please enter the number of messages you want to send:");

			RandomMessageGenerator messageGenerator = new RandomMessageGenerator();
			var randomMessages = messageGenerator.GetRandomSimpleMessages(n);

			foreach (var randomMessage in randomMessages)
				_channel.BasicPublish(
						exchange: "",
						routingKey: "nativereceiver",
						basicProperties: null,
						body: randomMessage.ToRabbitMQMessage()
					);
		}

		private async Task HandleSendOnlyNRandomAdvancedMessages()
		{
			var n = ConsoleUtils.GetUserNumberInput("Please enter the number of messages you want to send:");

			RandomMessageGenerator messageGenerator = new RandomMessageGenerator();
			var randomMessages = await messageGenerator.GetRandomAdvancedMessages(n);

			foreach (var randomMessage in randomMessages)
				_channel.BasicPublish(
						exchange: "",
						routingKey: "nativereceiver",
						basicProperties: null,
						body: randomMessage.ToRabbitMQMessage()
					);
		}
	}
}
