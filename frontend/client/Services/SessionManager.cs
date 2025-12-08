using sdk_client;
using sdk_client.Protocol;
using sdk_client.Services;
using System;

namespace client.Services
{
	public sealed class SessionManager : IDisposable
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
		private string? _currentSignalRHost;
		private int _currentSignalRPort;

		private SessionManager()
		{
		}

		public static SessionManager Instance => _instance.Value;

		public ApiClient? ApiClient => _apiClient;

		public ISignalRService? SignalRService => _signalRService;

		public LoginResponse? CurrentUser => _currentUser;

		public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(_currentUser.SessionToken);

		public void Initialize(string host, int port, int connectionTimeout = 30, int requestTimeout = 30,
			string? signalRHost = null, int? signalRPort = null)
		{
			var effectiveSignalRHost = signalRHost ?? host;
			var effectiveSignalRPort = signalRPort ?? 5001;

			// Only reinitialize if parameters changed or ApiClient doesn't exist
			if (_apiClient != null &&
			    _currentHost == host &&
			    _currentPort == port &&
			    _currentConnectionTimeout == connectionTimeout &&
			    _currentRequestTimeout == requestTimeout &&
			    _currentSignalRHost == effectiveSignalRHost &&
			    _currentSignalRPort == effectiveSignalRPort)
			{
				// Already initialized with same parameters, skip reinitialization
				return;
			}

			// Dispose old clients only if parameters changed
			if (_apiClient != null)
			{
				_apiClient.Dispose();
			}

			if (_signalRService != null)
			{
				_signalRService.Dispose();
			}

			_apiClient = new ApiClient(host, port, connectionTimeout, requestTimeout);
			_signalRService = new SignalRService(effectiveSignalRHost, effectiveSignalRPort);

			_currentHost = host;
			_currentPort = port;
			_currentConnectionTimeout = connectionTimeout;
			_currentRequestTimeout = requestTimeout;
			_currentSignalRHost = effectiveSignalRHost;
			_currentSignalRPort = effectiveSignalRPort;
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
		}
	}
}