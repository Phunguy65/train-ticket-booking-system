using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace sdk_client
{
	/// <summary>
	/// Manages TCP socket connections to the backend server.
	/// Provides methods for connecting, disconnecting, and sending/receiving data over TCP.
	/// </summary>
	public class TcpClientManager : IDisposable
	{
		private TcpClient _client;
		private NetworkStream _stream;
		private readonly string _host;
		private readonly int _port;
		private readonly int _connectionTimeout;
		private readonly int _requestTimeout;
		private bool _disposed;

		/// <summary>
		/// Initializes a new instance of TcpClientManager with server connection details.
		/// </summary>
		/// <param name="host">Server hostname or IP address</param>
		/// <param name="port">Server port number</param>
		/// <param name="connectionTimeout">Connection timeout in seconds</param>
		/// <param name="requestTimeout">Request timeout in seconds</param>
		public TcpClientManager(string host, int port, int connectionTimeout = 30, int requestTimeout = 30)
		{
			_host = host;
			_port = port;
			_connectionTimeout = connectionTimeout;
			_requestTimeout = requestTimeout;
		}

		/// <summary>
		/// Gets whether the client is currently connected to the server.
		/// </summary>
		public bool IsConnected
		{
			get { return _client != null && _client.Connected; }
		}

		/// <summary>
		/// Establishes a connection to the TCP server.
		/// </summary>
		public async Task ConnectAsync()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(TcpClientManager));
			}

			if (IsConnected)
			{
				return;
			}

			_client = new TcpClient();
			_client.SendTimeout = _requestTimeout * 1000;
			_client.ReceiveTimeout = _requestTimeout * 1000;

			var connectTask = _client.ConnectAsync(_host, _port);
			var timeoutTask = Task.Delay(_connectionTimeout * 1000);

			var completedTask = await Task.WhenAny(connectTask, timeoutTask).ConfigureAwait(false);

			if (completedTask == timeoutTask)
			{
				_client.Close();
				_client = null;
				throw new TimeoutException(
					$"Connection to {_host}:{_port} timed out after {_connectionTimeout} seconds.");
			}

			await connectTask.ConfigureAwait(false);
			_stream = _client.GetStream();
		}

		/// <summary>
		/// Sends data to the server asynchronously.
		/// </summary>
		/// <param name="data">String data to send</param>
		public async Task SendAsync(string data)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(TcpClientManager));
			}

			if (!IsConnected)
			{
				throw new InvalidOperationException("Not connected to server.");
			}

			var bytes = Encoding.UTF8.GetBytes(data);
			var sendTask = _stream.WriteAsync(bytes, 0, bytes.Length);
			var timeoutTask = Task.Delay(_requestTimeout * 1000);

			var completedTask = await Task.WhenAny(sendTask, timeoutTask).ConfigureAwait(false);

			if (completedTask == timeoutTask)
			{
				throw new TimeoutException($"Send operation timed out after {_requestTimeout} seconds.");
			}

			await sendTask.ConfigureAwait(false);
			await _stream.FlushAsync().ConfigureAwait(false);
		}

		/// <summary>
		/// Receives data from the server asynchronously.
		/// </summary>
		/// <returns>String data received from server</returns>
		public async Task<string> ReceiveAsync()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(TcpClientManager));
			}

			if (!IsConnected)
			{
				throw new InvalidOperationException("Not connected to server.");
			}

			var buffer = new byte[8192];
			var readTask = _stream.ReadAsync(buffer, 0, buffer.Length);
			var timeoutTask = Task.Delay(_requestTimeout * 1000);

			var completedTask = await Task.WhenAny(readTask, timeoutTask).ConfigureAwait(false);

			if (completedTask == timeoutTask)
			{
				throw new TimeoutException($"Receive operation timed out after {_requestTimeout} seconds.");
			}

			var bytesRead = await readTask.ConfigureAwait(false);

			if (bytesRead == 0)
			{
				throw new InvalidOperationException("Connection closed by server.");
			}

			return Encoding.UTF8.GetString(buffer, 0, bytesRead);
		}

		/// <summary>
		/// Disconnects from the server and releases network resources.
		/// </summary>
		public void Disconnect()
		{
			if (_stream != null)
			{
				_stream.Close();
				_stream = null;
			}

			if (_client != null)
			{
				_client.Close();
				_client = null;
			}
		}

		/// <summary>
		/// Releases all resources used by the TcpClientManager.
		/// </summary>
		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			Disconnect();
			_disposed = true;
		}
	}
}