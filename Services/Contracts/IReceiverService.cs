﻿namespace Services.Contracts
{
	/// <summary>
	/// Interface used for all receiver services
	/// </summary>
	public interface IReceiverService
	{
		/// <summary>
		/// Prepares receiver to process messages and starts processing
		/// </summary>
		/// <returns></returns>
		Task StartJob();

		/// <summary>
		/// Closes everything that was opened due to processing and clears the memory
		/// </summary>
		/// <returns></returns>
		Task FinishJob();
	}
}
