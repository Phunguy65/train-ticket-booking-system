namespace sdk_client.Protocol
{
	/// <summary>
	/// Represents a client request message in the TCP/Socket communication protocol.
	/// Contains the action to perform, request data, and a unique request identifier for tracking.
	/// </summary>
	public class Request
	{
		public string Action { get; set; } = string.Empty;
		public object Data { get; set; }
		public string RequestId { get; set; } = string.Empty;
	}
}

