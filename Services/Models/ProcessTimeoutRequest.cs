namespace Services.Models
{
	/// <summary>
	/// Model used to simulate N concurent clients
	/// </summary>
	public class ProcessTimeoutRequest : IMessage
	{
		/// <summary>
		/// Time for which receiver will sleep to simulate workload
		/// </summary>
		public int MillisecondsTimeout { get; set; }

		/// <summary>
		/// Name of the process/client to simplify analysis of simulation
		/// </summary>
		public string ProcessName { get; set; } = string.Empty;
	}
}
