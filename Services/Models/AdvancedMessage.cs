﻿using Azure.Messaging.ServiceBus;
using System.Text;
using System.Text.Json;
using Utils;

namespace Services.Models
{
	/// <summary>
	/// Model used to simulate bigger messages that need to be serialized before sending
	/// </summary>
	public class AdvancedMessage : IMessage
	{
		public string Name { get; set; } = string.Empty;
		public string Surname { get; set; } = string.Empty;
		public int Age { get; set; }
		public string Email { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public AdvancedMessageAddress Address { get; set; } = new AdvancedMessageAddress();

		public override string ToString()
		{
			var stringBuilder = new StringBuilder();

			stringBuilder.AppendLine($"Name: {Name}");
			stringBuilder.AppendLine($"Surname: {Surname}");
			stringBuilder.AppendLine($"Age: {Age}");
			stringBuilder.AppendLine($"Email: {Email}");
			stringBuilder.AppendLine($"Description: {Description}");

			stringBuilder.AppendLine("Address:");
			stringBuilder.AppendLine($"	StreetName: {Address.StreetName}");
			stringBuilder.AppendLine($"	BuildingNumber: {Address.BuildingNumber}");
			stringBuilder.AppendLine($"	City: {Address.City}");
			stringBuilder.AppendLine($"	PostalCode: {Address.PostalCode}");
			stringBuilder.AppendLine($"	Country: {Address.Country}");

			return stringBuilder.ToString();
		}
	}

	public class AdvancedMessageAddress
	{
		public string StreetName { get; set; } = string.Empty;
		public int BuildingNumber { get; set; }
		public string City { get; set; } = string.Empty;
		public string PostalCode { get; set; } = string.Empty;
		public string Country { get; set; } = string.Empty;
	}

	/// <summary>
	/// Mapper class to format advanced message to required format
	/// </summary>
	public static class AdvancedMessageMapper
	{
		public static ServiceBusMessage ToServiceBusMessage(this AdvancedMessage advancedMessage) => new ServiceBusMessage(JsonSerializer.Serialize(advancedMessage))
		{
			Subject = MessageType.AdvancedMessage.GetDescription()
		};

		public static byte[] ToRabbitMQMessage(this AdvancedMessage advancedMessage) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(advancedMessage));
	}
}
