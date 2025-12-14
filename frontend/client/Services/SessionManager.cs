using sdk_client;
using sdk_client.Protocol;
using sdk_client.Services;
using System;
using System.Threading.Tasks;

namespace client.Services
{
	public sealed class SessionManager : IDisposable, IAsyncDisposable
	{
		private static readonly Lazy<SessionManager> _instance = new Lazy<SessionManager>(() => new SessionManager());
		private ApiClient? _apiClient;
		private ISignalRService? _signalRService;
		private LoginResponse? _currentUser;
		private bool _disposed;
		private string? _currentHost;
		private int _currentPort;
		private int _currentConnectionTimeout;
		private int _currentRequestTimeout;
		private string? _currentSignalRUrl;

		private SessionManager()
		{
		}

		public static SessionManager Instance => _instance.Value;

		public ApiClient? ApiClient => _apiClient;

		public ISignalRService? SignalRService => _signalRService;

		public LoginResponse? CurrentUser => _currentUser;

		public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(_currentUser.SessionToken);

		public void Initialize(string host, int port, int connectionTimeout = 30, int requestTimeout = 30,
			string? signalRUrl = null)
		{
			var effectiveSignalRUrl = signalRUrl ?? "http://127.0.0.1:5001";

			if (_apiClient != null &&
			    _currentHost == host &&
			    _currentPort == port &&
			    _currentConnectionTimeout == connectionTimeout &&
			    _currentRequestTimeout == requestTimeout &&
			    _currentSignalRUrl == effectiveSignalRUrl)
			{
				return;
			}

			if (_apiClient != null)
			{
				_apiClient.Dispose();
			}

			if (_signalRService != null)
			{
				_signalRService.Dispose();
			}

			_apiClient = new ApiClient(host, port, connectionTimeout, requestTimeout);
			_signalRService = new SignalRService(effectiveSignalRUrl);

			_currentHost = host;
			_currentPort = port;
			_currentConnectionTimeout = connectionTimeout;
			_currentRequestTimeout = requestTimeout;
			_currentSignalRUrl = effectiveSignalRUrl;
		}

		public async Task InitializeAsync(string host, int port, int connectionTimeout = 30, int requestTimeout = 30,
			string? signalRUrl = null)
		{
			var effectiveSignalRUrl = signalRUrl ?? "http://127.0.0.1:5001";

			if (_apiClient != null &&
			    _currentHost == host &&
			    _currentPort == port &&
			    _currentConnectionTimeout == connectionTimeout &&
			    _currentRequestTimeout == requestTimeout &&
			    _currentSignalRUrl == effectiveSignalRUrl)
			{
				return;
			}

			if (_apiClient != null)
			{
				_apiClient.Dispose();
			}

			if (_signalRService != null)
			{
				await _signalRService.DisposeAsync().ConfigureAwait(false);
			}

			_apiClient = new ApiClient(host, port, connectionTimeout, requestTimeout);
			_signalRService = new SignalRService(effectiveSignalRUrl);

			_currentHost = host;
			_currentPort = port;
			_currentConnectionTimeout = connectionTimeout;
			_currentRequestTimeout = requestTimeout;
			_currentSignalRUrl = effectiveSignalRUrl;
		}

		public void SetSession(LoginResponse loginResponse)
		{
			_currentUser = loginResponse ?? throw new ArgumentNullException(nameof(loginResponse));

			if (_apiClient != null)
			{
				_apiClient.SessionToken = loginResponse.SessionToken;
			}
		}

		public void ClearSession()
		{
			_currentUser = null;

			if (_apiClient != null)
			{
				_apiClient.SessionToken = null;
			}
		}

		public async ValueTask DisposeAsync()
		{
			if (_disposed)
			{
				return;
			}

			_apiClient?.Dispose();
			_apiClient = null;

			if (_signalRService != null)
			{
				await _signalRService.DisposeAsync().ConfigureAwait(false);
				_signalRService = null;
			}

			_currentUser = null;
			_disposed = true;

			GC.SuppressFinalize(this);
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_apiClient?.Dispose();
			_apiClient = null;

			_signalRService?.Dispose();
			_signalRService = null;

			_currentUser = null;
			_disposed = true;

			GC.SuppressFinalize(this);
		}
	}
}