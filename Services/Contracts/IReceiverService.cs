namespace Services.Contracts
{
	public interface IReceiverService
	{
		/// <summary>
		/// Setup handlers that are needed to process message
		/// </summary>
		/// <returns></returns>
		void SetupHandlers();

		/// <summary>
		/// Start processing
		/// </summary>
		/// <returns></returns>
		Task StartProcessingAsync();

		/// <summary>
		/// Stop processing
		/// </summary>
		/// <returns></returns>
		Task StopProcessingAsync();

		/// <summary>
		/// Disposes everything that need to be disposed
		/// </summary>
		/// <returns></returns>
		Task DisposeAsync();
	}
}
