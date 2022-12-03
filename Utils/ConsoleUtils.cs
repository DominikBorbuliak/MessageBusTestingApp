namespace Utils
{
	public class ConsoleUtils
	{
		/// <summary>
		/// Writes line to console with specific color
		/// </summary>
		/// <param name="text">Text to be written</param>
		/// <param name="color">Color of the text</param>
		public static void WriteLineColor(string text, ConsoleColor color)
		{
			var oldColor = Console.ForegroundColor;

			Console.ForegroundColor = color;
			Console.WriteLine(text);
			Console.ForegroundColor = oldColor;
		}
	}
}
