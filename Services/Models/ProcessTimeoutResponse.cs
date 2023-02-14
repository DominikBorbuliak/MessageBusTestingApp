namespace Services.Models
{
	/// <summary>
	/// Model used to simulate N concurent clients
	/// </summary>
	public class ProcessTimeoutResponse : IMessage
	{
		/// <summary>
		/// Name of the process/client to simplify analysis of simulation
		/// </summary>
		public string ProcessName { get; set; } = string.Empty;
	}
}
