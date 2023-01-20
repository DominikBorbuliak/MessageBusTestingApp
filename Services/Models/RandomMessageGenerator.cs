using System.Collections.Generic;
using System.Text.Json;

namespace Services.Models
{
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

		public SimpleMessage GetRandomSimpleMessage(int minLength = 1, int maxLength = 256) => new SimpleMessage
		{
			Text = GenerateRandomText(minLength, maxLength)
		};

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

		private string GenerateRandomText(int minLength = 1, int maxLength = 256)
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

		private async Task<List<RandomAddressAPI>> GetRandomAddresses(int n)
		{
			try
			{
				var response = await _client.GetAsync($"{RandomDataApiAddressesEndpoint}?{string.Format(RandomDataApiSizeFormat, n)}");
				response.EnsureSuccessStatusCode();

				var responseBody = await response.Content.ReadAsStringAsync();

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

		private async Task<List<RandomUserAPI>> GetRandomUsers(int n)
		{
			try
			{
				var response = await _client.GetAsync($"{RandomDataApiUsersEndpoint}?{string.Format(RandomDataApiSizeFormat, n)}");
				response.EnsureSuccessStatusCode();

				var responseBody = await response.Content.ReadAsStringAsync();

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
