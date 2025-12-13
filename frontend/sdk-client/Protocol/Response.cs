namespace sdk_client.Protocol
{
	/// <summary>
	/// Represents a server response message in the TCP/Socket communication protocol.
	/// Contains success status, response data, error message, and the original request identifier.
	/// </summary>
	public class Response
	{
		public bool Success { get; set; }
		public object? Data { get; set; }
		public string? ErrorMessage { get; set; }
		public string RequestId { get; set; } = string.Empty;
	}
}