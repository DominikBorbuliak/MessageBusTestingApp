﻿using System.Text.Json.Serialization;

namespace Services.Data.Models
{
	/// <summary>
	/// Model used to gather random adresses from: https://random-data-api.com/api/v2/
	/// </summary>
	public class RandomAddressAPI
	{
		[JsonPropertyName("street_name")]
		public string StreetName { get; set; } = string.Empty;

		[JsonPropertyName("building_number")]
		public string BuildingNumber { get; set; } = string.Empty;

		[JsonPropertyName("city")]
		public string City { get; set; } = string.Empty;

		[JsonPropertyName("zip_code")]
		public string PostalCode { get; set; } = string.Empty;

		[JsonPropertyName("country")]
		public string Country { get; set; } = string.Empty;
	}
}
