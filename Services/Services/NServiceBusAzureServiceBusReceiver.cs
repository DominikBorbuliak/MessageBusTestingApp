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
				_endpointConfiguration.ExecuteTheseHandlersFirst(typeof(NServiceBusAzureServiceBusHandler));

				_endpointInstance = await Endpoint.Start(_endpointConfiguration);

				Console.WriteLine("Press any key to exit application and stop processing!");
				Console.ReadKey();

				await _endpointInstance.Stop();
			}
			finally
			{

			}
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
