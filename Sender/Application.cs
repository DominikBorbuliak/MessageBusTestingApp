using Services.Contracts;
using Services.Data;
using Services.Models;
using Services.View;
using Utils;

namespace Sender
{
	public class Application
	{
		private readonly ISenderService _senderService;

		public Application(ISenderService senderService)
		{
			_senderService = senderService;
		}

		/// <summary>
		/// Run the whole application
		/// </summary>
		/// <returns></returns>
		public async Task Run()
		{
			try
			{
				var actionMenu = new Menu<ActionType>("Please select action type.", "Use arrow DOWN and UP to navigate through menu.\nPress ENTER to submit.", true);
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
						case ActionType.SendAndReplyRectangularPrism:
							await HandleSendAndReplyRectangularPrism();
							break;
						case ActionType.SendAndReplySimulateNClients:
							await HandleSendAndReplySimulateNClients();
							break;
					}

					ConsoleUtils.WriteLineColor("Message was successfully send to queue!", ConsoleColor.Green);
					Thread.Sleep(5000);
				}
			}
			catch
			{
				ConsoleUtils.WriteLineColor($"Error occured. Please read the readme file, to check if you have everything setup correctly.", ConsoleColor.Red);
				ConsoleUtils.WriteLineColor($"Feel free to contact administrator via email '514127@mail.muni.cz' if the problem persists.", ConsoleColor.Red);
				Console.WriteLine();
				ConsoleUtils.WriteLineColor($"Press anything to exit application...", ConsoleColor.Red);
				Console.ReadKey();
			}
			finally
			{
				await _senderService.FinishJob();
			}
		}

		/// <summary>
		/// Sends one custom (filled by user) simple message
		/// </summary>
		/// <returns></returns>
		private async Task HandleSendOnlyOneCustomSimpleMessage()
		{
			var simpleMessage = new SimpleMessage
			{
				Text = ConsoleUtils.GetUserTextInput("Please insert text of message:")
			};

			await _senderService.SendSimpleMessage(simpleMessage);
		}

		/// <summary>
		/// Sends one custom (filled by user) advanced message
		/// </summary>
		/// <returns></returns>
		private async Task HandleSendOnlyOneCustomAdvancedMessage()
		{
			var advancedMessage = new AdvancedMessage
			{
				Name = ConsoleUtils.GetUserTextInput("Please insert the name:"),
				Surname = ConsoleUtils.GetUserTextInput("Please insert the surname:"),
				Age = ConsoleUtils.GetUserIntegerInput("Please enter the age:"),
				Email = ConsoleUtils.GetUserTextInput("Please enter the email:"),
				Description = ConsoleUtils.GetUserTextInput("Please enter the description:"),
				Address = new AdvancedMessageAddress
				{
					StreetName = ConsoleUtils.GetUserTextInput("Please enter the street name:"),
					BuildingNumber = ConsoleUtils.GetUserIntegerInput("Please enter the street number:"),
					City = ConsoleUtils.GetUserTextInput("Please enter the city:"),
					PostalCode = ConsoleUtils.GetUserTextInput("Please enter the postal code:"),
					Country = ConsoleUtils.GetUserTextInput("Please enter the country:")
				}
			};

			await _senderService.SendAdvancedMessage(advancedMessage);
		}

		/// <summary>
		/// Sends N randomly generated simple messages
		/// </summary>
		/// <returns></returns>
		private async Task HandleSendOnlyNRandomSimpleMessages()
		{
			var n = ConsoleUtils.GetUserIntegerInput("Please enter the number of messages you want to send:");

			RandomMessageGenerator messageGenerator = new RandomMessageGenerator();
			var randomMessages = messageGenerator.GetRandomSimpleMessages(n);

			foreach (var randomMessage in randomMessages)
				await _senderService.SendSimpleMessage(randomMessage);
		}

		/// <summary>
		/// Sends N randomly generated advanced messages
		/// </summary>
		/// <returns></returns>
		private async Task HandleSendOnlyNRandomAdvancedMessages()
		{
			var n = ConsoleUtils.GetUserIntegerInput("Please enter the number of messages you want to send:");

			RandomMessageGenerator messageGenerator = new RandomMessageGenerator();
			var randomMessages = await messageGenerator.GetRandomAdvancedMessages(n);

			foreach (var randomMessage in randomMessages)
				await _senderService.SendAdvancedMessage(randomMessage);
		}

		/// <summary>
		/// Semds one message that requires response
		/// </summary>
		/// <returns></returns>
		private async Task HandleSendAndReplyRectangularPrism()
		{
			var rectangularPrismRequest = new RectangularPrismRequest
			{
				EdgeA = ConsoleUtils.GetUserDoubleInput("Please insert the length of edge A:"),
				EdgeB = ConsoleUtils.GetUserDoubleInput("Please insert the length of edge B:"),
				EdgeC = ConsoleUtils.GetUserDoubleInput("Please insert the length of edge C:")
			};

			await _senderService.SendAndReplyRectangularPrism(rectangularPrismRequest);
		}

		/// <summary>
		/// Simulates N concurent clients
		/// </summary>
		/// <returns></returns>
		private async Task HandleSendAndReplySimulateNClients()
		{
			var n = ConsoleUtils.GetUserIntegerInput("Please enter the number of clients you want to simulate:");

			var processTimeoutRequests = new List<ProcessTimeoutRequest>();

			// Requests must be created separately, as it takes some time to fill in the required information, so the simulation would not be accurate.
			for (var i = 0; i < n; i++)
				processTimeoutRequests.Add(new ProcessTimeoutRequest
				{
					ProcessName = ConsoleUtils.GetUserTextInput("Please insert the name of client:"),
					MillisecondsTimeout = ConsoleUtils.GetUserIntegerInput("Please insert the timeout in miliseconds to simulate work:")
				});

			await Task.WhenAll(processTimeoutRequests.Select(processTimeoutRequest => _senderService.SendAndReplyProcessTimeout(processTimeoutRequest)));
		}
	}
}
