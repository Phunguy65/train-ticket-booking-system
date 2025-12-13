using System;

namespace sdk_client.Exceptions
{
	/// <summary>
	/// Exception thrown when an API request fails.
	/// Contains error details including the error message and request identifier.
	/// </summary>
	public class ApiException : Exception
	{
		/// <summary>
		/// Gets the error message from the server.
		/// </summary>
		public string ErrorMessage { get; }

		/// <summary>
		/// Gets the unique request identifier associated with the failed request.
		/// </summary>
		public string RequestId { get; }

		/// <summary>
		/// Initializes a new instance of ApiException with an error message and request ID.
		/// </summary>
		/// <param name="errorMessage">Error message from the server</param>
		/// <param name="requestId">Unique request identifier</param>
		public ApiException(string errorMessage, string requestId)
			: base(errorMessage)
		{
			ErrorMessage = errorMessage;
			RequestId = requestId;
		}

		/// <summary>
		/// Initializes a new instance of ApiException with an error message, request ID, and inner exception.
		/// </summary>
		/// <param name="errorMessage">Error message from the server</param>
		/// <param name="requestId">Unique request identifier</param>
		/// <param name="innerException">The exception that caused this exception</param>
		public ApiException(string errorMessage, string requestId, Exception innerException)
			: base(errorMessage, innerException)
		{
			ErrorMessage = errorMessage;
			RequestId = requestId;
		}
	}
}