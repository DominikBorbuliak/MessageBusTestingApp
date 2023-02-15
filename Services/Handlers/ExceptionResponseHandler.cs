using Services.Models;
using Utils;

namespace Services.Handlers
{
	/// <summary>
	/// Class used to handle exception response
	/// </summary>
	public static class ExceptionResponseHandler
	{
		/// <summary>
		/// Handles exception response
		/// </summary>
		/// <param name="exceptionResponse">Response to handle</param>
		/// <returns></returns>
		public static bool Handle(ExceptionResponse? exceptionResponse)
		{
			if (exceptionResponse != null)
			{
				ConsoleUtils.WriteLineColor(exceptionResponse.Text, ConsoleColor.Red);
				return true;
			}

			ConsoleUtils.WriteLineColor("No response found for: ExceptionResponse!", ConsoleColor.Red);
			return false;
		}
	}
}
