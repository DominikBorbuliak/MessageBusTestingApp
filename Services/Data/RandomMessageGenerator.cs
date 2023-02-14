using Services.Data.Models;
using Services.Models;
using System.Text.Json;

namespace Services.Data
{
	/// <summary>
	/// Class used to gather random adresses and users
	/// </summary>
	public class RandomMessageGenerator
	{
		private const string RandomDataApiBaseUrl = "https://random-data-api.com/api/v2/";
		private const string RandomDataApiAddressesEndpoint = "addresses";
		private const string RandomDataApiUsersEndpoint = "users";
		private const string RandomDataApiSizeFormat = "size={0}";
		private const string RandomCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";

		private readonly HttpClient _client;

		public RandomMessageGenerator()
		{
			_client = new HttpClient
			{
				BaseAddress = new Uri(RandomDataApiBaseUrl)
			};
		}

		/// <summary>
		/// Returns random simple messages
		/// </summary>
		/// <param name="n">Number of messages to return</param>
		/// <param name="minLength">Minimum length of text field</param>
		/// <param name="maxLength">Maximum length of text field</param>
		/// <returns></returns>
		public static List<SimpleMessage> GetRandomSimpleMessages(int n, int minLength = 1, int maxLength = 256)
		{
			var result = new List<SimpleMessage>();

			for (var i = 0; i < n; i++)
				result.Add(new SimpleMessage
				{
					Text = GenerateRandomText(minLength, maxLength)
				});

			return result;
		}

		/// <summary>
		/// Returns random advanced messages
		/// </summary>
		/// <param name="n">Number of messages to return</param>
		/// <returns></returns>
		public async Task<List<AdvancedMessage>> GetRandomAdvancedMessages(int n)
		{
			var randomUsers = await GetRandomUsers(n);
			var randomAddresses = await GetRandomAddresses(n);

			var result = new List<AdvancedMessage>();

			for (var i = 0; i < n; i++)
			{
				var randomUser = randomUsers[i];
				var randomAddress = randomAddresses[i];

				result.Add(new AdvancedMessage
				{
					Name = randomUser.Name,
					Surname = randomUser.Surname,
					Age = new Random().Next(1, 101),
					Email = randomUser.Email,
					Description = GenerateRandomText(),
					Address = new AdvancedMessageAddress
					{
						StreetName = randomAddress.StreetName,
						BuildingNumber = int.Parse(randomAddress.BuildingNumber),
						City = randomAddress.City,
						PostalCode = randomAddress.PostalCode,
						Country = randomAddress.Country
					}
				});
			}

			return result;
		}

		/// <summary>
		/// Generates random text from alphanumerical character
		/// </summary>
		/// <param name="minLength">Minimum length of text</param>
		/// <param name="maxLength">Maximum length of text</param>
		/// <returns></returns>
		private static string GenerateRandomText(int minLength = 1, int maxLength = 256)
		{
			var result = string.Empty;
			var random = new Random();
			var length = random.Next(minLength, maxLength);

			for (var i = 0; i < length; i++)
			{
				var position = random.Next(0, RandomCharacters.Length);
				result += RandomCharacters[position];
			}

			return result;
		}

		/// <summary>
		/// Gets random addresses
		/// If Random API fails, generate addresses with random texts
		/// </summary>
		/// <param name="n">Number of addresses to return</param>
		/// <returns></returns>
		private async Task<List<RandomAddressAPI>> GetRandomAddresses(int n)
		{
			try
			{
				var response = await _client.GetAsync($"{RandomDataApiAddressesEndpoint}?{string.Format(RandomDataApiSizeFormat, n)}");
				response.EnsureSuccessStatusCode();

				var responseBody = await response.Content.ReadAsStringAsync();

				// API returns different format of response for only 1 result and 2 or more
				// Throw exception if deserialization could not be performed to generate random address with totally random text and numbers
				if (n == 1)
					return new List<RandomAddressAPI> { JsonSerializer.Deserialize<RandomAddressAPI>(responseBody) ?? throw new Exception() };

				return JsonSerializer.Deserialize<List<RandomAddressAPI>>(responseBody) ?? throw new Exception();
			}
			catch
			{
				var result = new List<RandomAddressAPI>();

				for (var i = 0; i < n; i++)
				{
					result.Add(new RandomAddressAPI
					{
						StreetName = GenerateRandomText(),
						BuildingNumber = new Random().Next(1, 51).ToString(),
						City = GenerateRandomText(),
						PostalCode = GenerateRandomText(),
						Country = GenerateRandomText()
					});
				}

				return result;
			}

		}

		/// <summary>
		/// Gets random users
		/// If Random API fails, generate users with random texts
		/// </summary>
		/// <param name="n">Number of users to return</param>
		/// <returns></returns>
		private async Task<List<RandomUserAPI>> GetRandomUsers(int n)
		{
			try
			{
				var response = await _client.GetAsync($"{RandomDataApiUsersEndpoint}?{string.Format(RandomDataApiSizeFormat, n)}");
				response.EnsureSuccessStatusCode();

				var responseBody = await response.Content.ReadAsStringAsync();

				// API returns different format of response for only 1 result and 2 or more
				// Throw exception if deserialization could not be performed to generate random address with totally random text and numbers
				if (n == 1)
					return new List<RandomUserAPI> { JsonSerializer.Deserialize<RandomUserAPI>(responseBody) ?? throw new Exception() };

				return JsonSerializer.Deserialize<List<RandomUserAPI>>(responseBody) ?? throw new Exception();
			}
			catch
			{
				var result = new List<RandomUserAPI>();

				for (var i = 0; i < n; i++)
				{
					result.Add(new RandomUserAPI
					{
						Name = GenerateRandomText(1, 51),
						Surname = GenerateRandomText(1, 51),
						Email = GenerateRandomText(1, 101)
					});
				}

				return result;
			}
		}
	}
}
