using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Models;
using Utils;

namespace Services.Services
{
	public class NServiceBusSender : ISenderService
	{
		private readonly IEndpointInstance _endpointInstance;

		public NServiceBusSender(IConfiguration configuration, bool isAzureServiceBus)
		{
			var endpointConfiguration = new EndpointConfiguration(configuration.GetSection("ConnectionSettings")["SenderEndpointName"]);

			if (isAzureServiceBus)
			{
				var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();

				transport.ConnectionString(configuration.GetConnectionString("AzureServiceBus"));
				transport.Routing().RouteToEndpoint(typeof(SimpleMessage), configuration.GetSection("ConnectionSettings")["ReceiverEndpointName"]);
				transport.Routing().RouteToEndpoint(typeof(AdvancedMessage), configuration.GetSection("ConnectionSettings")["ReceiverEndpointName"]);
				transport.TopicName(configuration.GetSection("ConnectionSettings")["TopicName"]);

				endpointConfiguration.SendOnly();
			}
			else
			{
				var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();

				transport.UseConventionalRoutingTopology(QueueType.Quorum);
				transport.ConnectionString($"host={configuration.GetSection("ConnectionSettings")["HostName"]}");
				transport.Routing().RouteToEndpoint(typeof(SimpleMessage), configuration.GetSection("ConnectionSettings")["ReceiverEndpointName"]);
				transport.Routing().RouteToEndpoint(typeof(AdvancedMessage), configuration.GetSection("ConnectionSettings")["ReceiverEndpointName"]);
			}

			endpointConfiguration.EnableInstallers();
			_endpointInstance = Endpoint.Start(endpointConfiguration).Result;
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
				await _endpointInstance.Stop();
			}
		}

		private async Task HandleSendOnlyOneCustomSimpleMessage()
		{
			var simpleMessage = new SimpleMessage
			{
				Text = ConsoleUtils.GetUserTextInput("Please insert text of message:")
			};

			await _endpointInstance.Send(simpleMessage);
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

			await _endpointInstance.Send(advancedMessage);
		}

		private async Task HandleSendOnlyNRandomSimpleMessages()
		{
			var n = ConsoleUtils.GetUserNumberInput("Please enter the number of messages you want to send:");

			RandomMessageGenerator messageGenerator = new RandomMessageGenerator();
			var randomMessages = messageGenerator.GetRandomSimpleMessages(n);

			foreach (var randomMessage in randomMessages)
				await _endpointInstance.Send(randomMessage);
		}

		private async Task HandleSendOnlyNRandomAdvancedMessages()
		{
			var n = ConsoleUtils.GetUserNumberInput("Please enter the number of messages you want to send:");

			RandomMessageGenerator messageGenerator = new RandomMessageGenerator();
			var randomMessages = await messageGenerator.GetRandomAdvancedMessages(n);

			foreach (var randomMessage in randomMessages)
				await _endpointInstance.Send(randomMessage);
		}
	}
}
