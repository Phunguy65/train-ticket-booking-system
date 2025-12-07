using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using sdk_client.Exceptions;
using sdk_client.Protocol;
using System;
using System.Threading.Tasks;

namespace sdk_client
{
	/// <summary>
	/// High-level API client for communicating with the backend server.
	/// Provides a clean interface for making API calls with automatic session management.
	/// </summary>
	public class ApiClient : IDisposable
	{
		private readonly TcpClientManager _tcpClient;
		private readonly JsonSerializerSettings _jsonSettings;
		private bool _disposed;

		/// <summary>
		/// Gets or sets the current session token for authenticated requests.
		/// </summary>
		public string? SessionToken { get; set; }

		/// <summary>
		/// Initializes a new instance of ApiClient with server connection details.
		/// </summary>
		/// <param name="host">Server hostname or IP address</param>
		/// <param name="port">Server port number</param>
		/// <param name="connectionTimeout">Connection timeout in seconds</param>
		/// <param name="requestTimeout">Request timeout in seconds</param>
		public ApiClient(string host = "127.0.0.1", int port = 5000, int connectionTimeout = 30,
			int requestTimeout = 30)
		{
			_tcpClient = new TcpClientManager(host, port, connectionTimeout, requestTimeout);

			// Configure JSON serialization for UTC timezone handling
			_jsonSettings = new JsonSerializerSettings
			{
				DateTimeZoneHandling = DateTimeZoneHandling.Utc,
				DateFormatHandling = DateFormatHandling.IsoDateFormat
			};
		}

		/// <summary>
		/// Gets whether the client is currently connected to the server.
		/// </summary>
		public bool IsConnected
		{
			get { return _tcpClient.IsConnected; }
		}

		/// <summary>
		/// Establishes a connection to the server.
		/// </summary>
		public async Task ConnectAsync()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(ApiClient));
			}

			await _tcpClient.ConnectAsync().ConfigureAwait(false);
		}

		/// <summary>
		/// Sends a request to the server and returns the response.
		/// Automatically injects SessionToken for authenticated endpoints.
		/// </summary>
		/// <param name="action">Action string in format 'Category.Action'</param>
		/// <param name="data">Request data object</param>
		/// <returns>Response object from server</returns>
		public async Task<Response> SendRequestAsync(string action, object? data = null)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(ApiClient));
			}

			if (!IsConnected)
			{
				await ConnectAsync().ConfigureAwait(false);
			}

			var requestId = Guid.NewGuid().ToString();

			var requestData = data;
			if (!string.IsNullOrEmpty(SessionToken) && data != null)
			{
				var jObject = JObject.FromObject(data);
				var existingToken = jObject["SessionToken"]?.ToString();
				if (string.IsNullOrEmpty(existingToken))
				{
					jObject["SessionToken"] = SessionToken;
				}

				requestData = jObject;
			}
			else if (!string.IsNullOrEmpty(SessionToken) && data == null)
			{
				requestData = new { SessionToken };
			}

			var request = new Request { Action = action, Data = requestData, RequestId = requestId };

			var requestJson = JsonConvert.SerializeObject(request, _jsonSettings);
			await _tcpClient.SendAsync(requestJson).ConfigureAwait(false);

			var responseJson = await _tcpClient.ReceiveAsync().ConfigureAwait(false);
			var response = JsonConvert.DeserializeObject<Response>(responseJson, _jsonSettings);

			if (response == null)
			{
				throw new ApiException("Failed to deserialize server response.", requestId);
			}

			if (!response.Success)
			{
				throw new ApiException(response.ErrorMessage ?? "Unknown error occurred.", requestId);
			}

			return response;
		}

		/// <summary>
		/// Sends a request and deserializes the response data to a specific type.
		/// </summary>
		/// <typeparam name="T">Type to deserialize response data to</typeparam>
		/// <param name="action">Action string in format 'Category.Action'</param>
		/// <param name="data">Request data object</param>
		/// <returns>Deserialized response data</returns>
		public async Task<T?> SendRequestAsync<T>(string action, object? data = null)
		{
			var response = await SendRequestAsync(action, data).ConfigureAwait(false);

			if (response.Data is JObject jObject)
			{
				return jObject.ToObject<T>();
			}

			if (response.Data is JArray jArray)
			{
				return jArray.ToObject<T>();
			}

			return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(response.Data));
		}

		/// <summary>
		/// Disconnects from the server.
		/// </summary>
		public void Disconnect()
		{
			_tcpClient.Disconnect();
		}

		/// <summary>
		/// Releases all resources used by the ApiClient.
		/// </summary>
		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_tcpClient.Dispose();
			_disposed = true;
		}
	}
}