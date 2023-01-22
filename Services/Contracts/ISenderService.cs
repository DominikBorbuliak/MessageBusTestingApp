using Services.Models;

namespace Services.Contracts
{
	public interface ISenderService
	{
		Task SendSimpleMessage(SimpleMessage simpleMessage);
		Task SendAdvancedMessage(AdvancedMessage advancedMessage);
		Task FinishJob();
	}
}
