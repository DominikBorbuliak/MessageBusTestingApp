using Microsoft.Extensions.Configuration;
using Services.Contracts;
using Services.Models;
using Utils;

namespace Services.Services
{
	public class NServiceBusAzureServiceBusReceiver : IReceiverService
	{
		private readonly EndpointConfiguration _endpointConfiguration;
		private IEndpointInstance _endpointInstance = null!;

		public NServiceBusAzureServiceBusReceiver(IConfiguration configuration)
		{
			_endpointConfiguration = new EndpointConfiguration(configuration.GetSection("ConnectionSettings")["ReceiverEndpointName"]);

			var transport = _endpointConfiguration.UseTransport<AzureServiceBusTransport>();
			transport.ConnectionString(configuration.GetConnectionString("AzureServiceBus"));
			transport.TopicName(configuration.GetSection("ConnectionSettings")["TopicName"]);

			_endpointConfiguration.EnableInstallers();
		}

		public async Task Run()
		{
			try
			{
				_endpointConfiguration.ExecuteTheseHandlersFirst(typeof(NServiceBusAzureServiceBusSimpleMessageHandler));
				_endpointConfiguration.ExecuteTheseHandlersFirst(typeof(NServiceBusAzureServiceBusAdvancedMessageHandler));

				_endpointInstance = await Endpoint.Start(_endpointConfiguration);

				Console.WriteLine("Press any key to exit application and stop processing!");
				Console.ReadKey();
			}
			finally
			{
				await _endpointInstance.Stop();
			}
		}
	}
	public class NServiceBusAzureServiceBusSimpleMessageHandler : IHandleMessages<SimpleMessage>
	{
		public Task Handle(SimpleMessage message, IMessageHandlerContext context)
		{
			ConsoleUtils.WriteLineColor($"Simple messsage received: {message.Text}", ConsoleColor.Green);
			return Task.CompletedTask;
		}
	}

	public class NServiceBusAzureServiceBusAdvancedMessageHandler : IHandleMessages<AdvancedMessage>
	{
		public Task Handle(AdvancedMessage message, IMessageHandlerContext context)
		{
			ConsoleUtils.WriteLineColor($"Advanced messsage received: {message}", ConsoleColor.Green);
			return Task.CompletedTask;
		}
	}
}
