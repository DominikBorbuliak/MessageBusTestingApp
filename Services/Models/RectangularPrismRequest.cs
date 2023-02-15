using System.Text;

namespace Services.Models
{
	/// <summary>
	/// Model used to simulate send and reply pattern and exception during processing
	/// </summary>
	public class RectangularPrismRequest : IMessage
	{
		public double EdgeA { get; set; }

		public double EdgeB { get; set; }

		public double EdgeC { get; set; }

		/// <summary>
		/// Text of the exception that will be thrown in receiver
		/// </summary>
		public string ExceptionText { get; set; } = string.Empty;

		/// <summary>
		/// Number of attempt on which reciever should successfuly process message
		/// 0 or less - never
		/// 1 - first attempt
		/// </summary>
		public int SucceedOn { get; set; }

		public override string ToString()
		{
			var stringBuilder = new StringBuilder();

			stringBuilder.AppendLine($"Edge A: {EdgeA}");
			stringBuilder.AppendLine($"Edge B: {EdgeB}");
			stringBuilder.AppendLine($"Edge C: {EdgeC}");

			return stringBuilder.ToString();
		}
	}
}
