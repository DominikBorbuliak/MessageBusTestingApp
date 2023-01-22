namespace Services.Contracts
{
	public interface IReceiverService
	{
		Task StartJob();
		Task FinishJob();
	}
}
