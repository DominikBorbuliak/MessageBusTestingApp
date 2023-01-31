﻿using Services.Contracts;
using Services.Data;
using Services.Models;
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
			catch (Exception ex)
			{
				ConsoleUtils.WriteLineColor($"Error occured. Press anything to exit application.", ConsoleColor.Red);
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

		private async Task HandleSendAndReplySimulateNClients()
		{
			var n = ConsoleUtils.GetUserIntegerInput("Please enter the number of clients you want to simulate:");
			var names = new List<string>();
			var timeouts = new List<int>();

			for (var i = 0; i < n; i++)
			{
				names.Add(ConsoleUtils.GetUserTextInput("Please insert the name of process:"));
				timeouts.Add(ConsoleUtils.GetUserIntegerInput("Please insert the timeout for process in miliseconds:"));
			}

			var tasks = new List<Task>();

			for (var i = 0; i < n; i++)
			{
				tasks.Add(_senderService.SendAndReplyProcessTimeout(new ProcessTimeoutRequest
				{
					ProcessName = names[i],
					MillisecondsTimeout = timeouts[i]
				}));
			}

			await Task.WhenAll(tasks);
		}
	}
}
