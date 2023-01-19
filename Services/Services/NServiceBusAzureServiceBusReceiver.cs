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

		public void SetupHandlers()
		{
			_endpointConfiguration.ExecuteTheseHandlersFirst(typeof(NServiceBusAzureServiceBusHandler));
		}

		public async Task StartProcessingAsync()
		{
			_endpointInstance = await Endpoint.Start(_endpointConfiguration);
		}

		public async Task StopProcessingAsync()
		{
			await _endpointInstance.Stop();
		}

		public Task DisposeAsync()
		{
			return Task.CompletedTask;
		}
	}
	public class NServiceBusAzureServiceBusHandler : IHandleMessages<Message>
	{
		public Task Handle(Message message, IMessageHandlerContext context)
		{
			ConsoleUtils.WriteLineColor($"Messsage received: {message.Text}", ConsoleColor.Green);
			return Task.CompletedTask;
		}
	}
}
