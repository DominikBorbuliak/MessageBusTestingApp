using Services.Contracts;
using Services.Data;
using Services.Models;
using Services.Services;
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
						case ActionType.SendOnlySimulateException:
							await HandleSendOnlySimulateException();
							break;
						case ActionType.SendAndReplyWaitRectangularPrism:
							await HandleSendAndReplyRectangularPrism(true);
							break;
						case ActionType.SendAndReplyWaitSimulateNClients:
							await HandleSendAndReplySimulateNClients(true);
							break;
						case ActionType.SendAndReplyWaitSimulateException:
							await HandleSendAndReplySimulateException(true);
							break;
						case ActionType.SendAndReplyNoWaitRectangularPrism:
							await HandleSendAndReplyRectangularPrism(false);
							break;
						case ActionType.SendAndReplyNoWaitSimulateNClients:
							await HandleSendAndReplySimulateNClients(false);
							break;
						case ActionType.SendAndReplyNoWaitSimulateException:
							await HandleSendAndReplySimulateException(false);
							break;
					}

					ConsoleUtils.WriteLineColor("Press anything to continue to the menu.", ConsoleColor.Green);
					Console.ReadKey(true);
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
				Text = ConsoleUtils.GetUserTextInput("Please enter text of the message:")
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
				Name = ConsoleUtils.GetUserTextInput("Please enter the name:"),
				Surname = ConsoleUtils.GetUserTextInput("Please enter the surname:"),
				Age = ConsoleUtils.GetUserIntegerInput("Please enter the age:"),
				Email = ConsoleUtils.GetUserTextInput("Please enter the email:"),
				Description = ConsoleUtils.GetUserTextInput("Please enter the description:"),
				Address = new AdvancedMessageAddress
				{
					StreetName = ConsoleUtils.GetUserTextInput("Please enter the street name:"),
					BuildingNumber = ConsoleUtils.GetUserIntegerInput("Please enter the building number:"),
					City = ConsoleUtils.GetUserTextInput("Please enter the city:"),
					PostalCode = ConsoleUtils.GetUserTextInput("Please enter the postal code:"),
					Country = ConsoleUtils.GetUserTextInput("Please enter the country:")
				}
			};

			await _senderService.SendAdvancedMessage(advancedMessage);
		}

		/// <summary>
		/// Sends N randomly generated simple messages at the same time
		/// </summary>
		/// <returns></returns>
		private async Task HandleSendOnlyNRandomSimpleMessages()
		{
			var n = ConsoleUtils.GetUserIntegerInput("Please enter the number of messages you want to send:");

			var randomMessages = RandomMessageGenerator.GetRandomSimpleMessages(n);

			await Task.WhenAll(randomMessages.Select(randomMessage => _senderService.SendSimpleMessage(randomMessage)));
		}

		/// <summary>
		/// Sends N randomly generated advanced messages at the same time
		/// </summary>
		/// <returns></returns>
		private async Task HandleSendOnlyNRandomAdvancedMessages()
		{
			var n = ConsoleUtils.GetUserIntegerInput("Please enter the number of messages you want to send:");

			var randomMessages = await new RandomMessageGenerator().GetRandomAdvancedMessages(n);

			await Task.WhenAll(randomMessages.Select(randomMessage => _senderService.SendAdvancedMessage(randomMessage)));
		}

		/// <summary>
		/// Simulates exception on receiver end - Send Only
		/// </summary>
		/// <returns></returns>
		private async Task HandleSendOnlySimulateException()
		{
			var exceptionMessage = new ExceptionMessage
			{
				SucceedOn = ConsoleUtils.GetUserIntegerInput("Please enter the number (1 is first try, 0 or less to never succeed) of attempt on which it will succeed:"),
				ExceptionText = ConsoleUtils.GetUserTextInput("Please enter the text of the exception:")
			};

			await _senderService.SendExceptionMessage(exceptionMessage);
		}

		/// <summary>
		/// Sends one message that requires response
		/// </summary>
		/// <returns></returns>
		private async Task HandleSendAndReplyRectangularPrism(bool wait)
		{
			if (wait && (typeof(RabbitMQSenderService) == _senderService.GetType() || typeof(NServiceBusSenderService) == _senderService.GetType()))
			{
				ConsoleUtils.WriteLineColor("Solution not found!", ConsoleColor.Yellow);
				return;
			}

			var rectangularPrismRequest = new RectangularPrismRequest
			{
				EdgeA = ConsoleUtils.GetUserDoubleInput("Please enter the length of edge A:"),
				EdgeB = ConsoleUtils.GetUserDoubleInput("Please enter the length of edge B:"),
				EdgeC = ConsoleUtils.GetUserDoubleInput("Please enter the length of edge C:"),
				SucceedOn = 1
			};

			await _senderService.SendAndReplyRectangularPrism(rectangularPrismRequest, wait);
		}

		/// <summary>
		/// Simulates N concurent clients
		/// </summary>
		/// <returns></returns>
		private async Task HandleSendAndReplySimulateNClients(bool wait)
		{
			if (wait && (typeof(RabbitMQSenderService) == _senderService.GetType() || typeof(NServiceBusSenderService) == _senderService.GetType()))
			{
				ConsoleUtils.WriteLineColor("Solution not found!", ConsoleColor.Yellow);
				return;
			}

			var n = ConsoleUtils.GetUserIntegerInput("Please enter the number of clients you want to simulate:");

			var processTimeoutRequests = new List<ProcessTimeoutRequest>();

			// Requests must be created separately, as it takes some time to fill in the required information, so the simulation would not be accurate.
			for (var i = 1; i <= n; i++)
				processTimeoutRequests.Add(new ProcessTimeoutRequest
				{
					ProcessName = ConsoleUtils.GetUserTextInput($"Please enter the name of {i}. client:"),
					MillisecondsTimeout = ConsoleUtils.GetUserIntegerInput("Please enter the timeout in miliseconds to simulate work:")
				});

			await Task.WhenAll(processTimeoutRequests.Select(processTimeoutRequest => _senderService.SendAndReplyProcessTimeout(processTimeoutRequest, wait)));
		}

		/// <summary>
		/// Simulates exception on receiver end - Send & Reply
		/// </summary>
		/// <returns></returns>
		private async Task HandleSendAndReplySimulateException(bool wait)
		{
			if (wait && (typeof(RabbitMQSenderService) == _senderService.GetType() || typeof(NServiceBusSenderService) == _senderService.GetType()))
			{
				ConsoleUtils.WriteLineColor("Solution not found!", ConsoleColor.Yellow);
				return;
			}

			var rectangularPrismRequest = new RectangularPrismRequest
			{
				EdgeA = ConsoleUtils.GetUserDoubleInput("Please enter the length of edge A:"),
				EdgeB = ConsoleUtils.GetUserDoubleInput("Please enter the length of edge B:"),
				EdgeC = ConsoleUtils.GetUserDoubleInput("Please enter the length of edge C:"),
				SucceedOn = ConsoleUtils.GetUserIntegerInput("Please enter the number (1 is first try, 0 or less to never succeed) of attempt on which it will succeed:"),
				ExceptionText = ConsoleUtils.GetUserTextInput("Please enter the text of the exception:")
			};

			await _senderService.SendAndReplyRectangularPrism(rectangularPrismRequest, wait);
		}
	}
}
