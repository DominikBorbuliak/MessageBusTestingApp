using Services.Models;
using Utils;

namespace Services.View
{
	/// <summary>
	/// Class used to display intro console and setup console
	/// </summary>
	public static class Setup
	{

		public const string ApplicationName = @"
___  ___                                       ______             
|  \/  |                                       | ___ \            
| .  . |  ___  ___  ___   __ _   __ _   ___    | |_/ / _   _  ___ 
| |\/| | / _ \/ __|/ __| / _` | / _` | / _ \   | ___ \| | | |/ __|
| |  | ||  __/\__ \\__ \| (_| || (_| ||  __/   | |_/ /| |_| |\__ \
\_|  |_/ \___||___/|___/ \__,_| \__, | \___|   \____/  \__,_||___/
                                 __/ |                            
                                |___/                             
 _____             _    _                    ___                  
|_   _|           | |  (_)                  / _ \                 
  | |    ___  ___ | |_  _  _ __    __ _    / /_\ \ _ __   _ __    
  | |   / _ \/ __|| __|| || '_ \  / _` |   |  _  || '_ \ | '_ \   
  | |  |  __/\__ \| |_ | || | | || (_| |   | | | || |_) || |_) |  
  \_/   \___||___/ \__||_||_| |_| \__, |   \_| |_/| .__/ | .__/   
                                   __/ |          | |    | |      
                                  |___/           |_|    |_|      
";

		public const string AuthorLine = "Created by: Dominik Borbuliak";
		public const string AppPurposeLine = "The main purpose of this application is to test and analyze different types of existing message bus architectures.";
		public const string ContinueLine = "Please press anything to continue to the application...";
		public const string PromptAfterMainMenu = "Use arrow DOWN and UP to navigate through menu.\nPress ENTER to submit.";
		public const string PromptBeforeMainMenu = "Please select message bus type for: {0}";

		public static MessageBusType? Run(string consoleTitle)
		{
			// Setup title
			Console.Title = consoleTitle;

			// Setup colors and cursor visibility
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.White;
			Console.CursorVisible = false;

			// Display intro 
			ConsoleUtils.WriteLineColor(ApplicationName, ConsoleColor.Cyan);
			ConsoleUtils.WriteLineColor(AuthorLine, ConsoleColor.DarkCyan);
			Console.WriteLine();
			ConsoleUtils.WriteLineColor(AppPurposeLine, ConsoleColor.DarkCyan);
			Console.WriteLine();
			Console.WriteLine(ContinueLine);

			Console.ReadKey();

			// Display main menu
			var mainMenu = new Menu<MessageBusType>(string.Format(PromptBeforeMainMenu, consoleTitle), PromptAfterMainMenu, true);
			var pickedMainMenuItem = mainMenu.HandleMenuMovement();

			// Change title only if EXIT was not picked from main menu
			if (pickedMainMenuItem != null)
				Console.Title = $"{((MessageBusType)pickedMainMenuItem).GetMenuDisplayName()} {consoleTitle}";

			Console.Clear();

			return pickedMainMenuItem;
		}
	}
}
