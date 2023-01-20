using System.Text.Json.Serialization;

namespace Services.Models
{
	public class RandomUserAPI
	{
		[JsonPropertyName("first_name")]
		public string Name { get; set; } = string.Empty;

		[JsonPropertyName("last_name")]
		public string Surname { get; set; } = string.Empty;

		[JsonPropertyName("email")]
		public string Email { get; set; } = string.Empty;
	}
}
