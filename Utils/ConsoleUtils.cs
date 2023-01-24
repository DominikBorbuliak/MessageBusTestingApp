namespace Utils
{
	public class ConsoleUtils
	{
		/// <summary>
		/// Writes line to console with specific color of text
		/// </summary>
		/// <param name="text">Text to be written</param>
		/// <param name="textColor">Color of the text</param>
		public static void WriteLineColor(string text, ConsoleColor textColor)
		{
			var oldTextColor = Console.ForegroundColor;

			Console.ForegroundColor = textColor;
			Console.WriteLine(text);
			Console.ForegroundColor = oldTextColor;
		}

		/// <summary>
		/// Writes line to console with specific color of text and background
		/// </summary>
		/// <param name="text">Text to be written</param>
		/// <param name="textColor">Color of the text</param>
		/// <param name="backgroundColor">Color of the background</param>
		public static void WriteLineColor(string text, ConsoleColor textColor, ConsoleColor backgroundColor)
		{
			var oldTextColor = Console.ForegroundColor;
			var oldBackgroundColor = Console.BackgroundColor;

			Console.ForegroundColor = textColor;
			Console.BackgroundColor = backgroundColor;
			Console.WriteLine(text);
			Console.ForegroundColor = oldTextColor;
			Console.BackgroundColor = oldBackgroundColor;
		}

		/// <summary>
		/// Gets text input from user
		/// </summary>
		/// <param name="prompt">Promt to be asked</param>
		/// <returns></returns>
		public static string GetUserTextInput(string prompt)
		{
			string? input = null;

			while (string.IsNullOrEmpty(input))
			{
				Console.WriteLine(prompt);
				input = Console.ReadLine();
			}

			return input;
		}

		/// <summary>
		/// Gets integer input from user
		/// </summary>
		/// <param name="prompt">Promt to be asked</param>
		/// <returns></returns>
		public static int GetUserIntegerInput(string prompt)
		{
			string? input = null;

			while (string.IsNullOrEmpty(input) || !int.TryParse(input, out _))
			{
				Console.WriteLine(prompt);
				input = Console.ReadLine();
			}

			return int.Parse(input);
		}

		/// <summary>
		/// Gets double input from user
		/// </summary>
		/// <param name="prompt">Promt to be asked</param>
		/// <returns></returns>
		public static double GetUserDoubleInput(string prompt)
		{
			string? input = null;

			while (string.IsNullOrEmpty(input) || !double.TryParse(input, out _))
			{
				Console.WriteLine(prompt);
				input = Console.ReadLine();
			}

			return double.Parse(input);
		}
	}
}
