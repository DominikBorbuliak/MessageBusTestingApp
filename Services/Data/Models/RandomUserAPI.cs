using System.Text.Json.Serialization;

namespace Services.Data.Models
{
	/// <summary>
	/// Model used to gather random users from: https://random-data-api.com/api/v2/
	/// </summary>
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
