using Azure.Messaging.ServiceBus;
using System.Text;
using System.Text.Json;
using Utils;

namespace Services.Models
{
	/// <summary>
	/// Model used to simulate thrown exception during processing - Send Only
	/// </summary>
	public class ExceptionMessage : IMessage
	{
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
	}

	/// <summary>
	/// Mapper class to format simple message to required format
	/// </summary>
	public static class ExceptionMessageMapper
	{
		/// <summary>
		/// Formats ExceptionMessage to ServiceBusMessage
		/// </summary>
		/// <param name="exceptionMessage"></param>
		/// <returns></returns>
		public static ServiceBusMessage ToServiceBusMessage(this ExceptionMessage exceptionMessage) => new(JsonSerializer.Serialize(exceptionMessage))
		{
			Subject = MessageType.ExceptionMessage.GetDescription()
		};

		/// <summary>
		/// Formats ExceptionMessage to RabbitMQ message
		/// </summary>
		/// <param name="exceptionMessage"></param>
		/// <returns></returns>
		public static byte[] ToRabbitMQMessage(this ExceptionMessage exceptionMessage) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(exceptionMessage));
	}

	/// <summary>
	/// Handler class to handle exception message
	/// </summary>
	public static class ExceptionMessageHandler
	{
		/// <summary>
		/// Handles exception message
		/// Throws exception if simulating error
		/// </summary>
		/// <param name="exceptionMessage">Message to handle</param>
		/// <param name="deliveryCount">Current delivery attempt number</param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static bool Handle(ExceptionMessage? exceptionMessage, int deliveryCount)
		{
			if (exceptionMessage == null)
			{
				ConsoleUtils.WriteLineColor("ExceptionMessage could not be deserialized correctly!", ConsoleColor.Red);
				return false;
			}

			if (exceptionMessage.SucceedOn <= 0 || deliveryCount < exceptionMessage.SucceedOn)
			{
				ConsoleUtils.WriteLineColor($"Throwing exception with text: {exceptionMessage.ExceptionText}", ConsoleColor.Yellow);
				throw new Exception(exceptionMessage.ExceptionText);
			}

			ConsoleUtils.WriteLineColor($"Exception messsage with text: {exceptionMessage.ExceptionText} succeeded!", ConsoleColor.Green);

			return true;
		}
	}
}
