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
			if (exceptionResponse == null)
			{
				ConsoleUtils.WriteLineColor("ExceptionResponse could not be deserialized correctly!", ConsoleColor.Red);
				return false;
			}

			ConsoleUtils.WriteLineColor(exceptionResponse.Text, ConsoleColor.Red);
			return true;
		}
	}
}
