using System.Text;

namespace Services.Models
{
	/// <summary>
	/// Model used to simulate send and reply pattern and exception during processing
	/// </summary>
	public class RectangularPrismResponse : IMessage
	{
		public double SurfaceArea { get; set; }

		public double Volume { get; set; }

		public override string ToString()
		{
			var stringBuilder = new StringBuilder();

			stringBuilder.AppendLine($"Surface Area: {SurfaceArea}");
			stringBuilder.AppendLine($"Volume: {Volume}");

			return stringBuilder.ToString();
		}
	}
}
