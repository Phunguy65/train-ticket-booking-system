using sdk_client;
using sdk_client.Protocol;
using System;

namespace client.Services
{
	public sealed class SessionManager : IDisposable
	{
		private static readonly Lazy<SessionManager> _instance = new Lazy<SessionManager>(() => new SessionManager());
		private ApiClient? _apiClient;
		private LoginResponse? _currentUser;
		private bool _disposed;

		private SessionManager()
		{
		}

		public static SessionManager Instance => _instance.Value;

		public ApiClient? ApiClient => _apiClient;

		public LoginResponse? CurrentUser => _currentUser;

		public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(_currentUser.SessionToken);

		public void Initialize(string host, int port, int connectionTimeout = 30, int requestTimeout = 30)
		{
			if (_apiClient != null)
			{
				_apiClient.Dispose();
			}

			_apiClient = new ApiClient(host, port, connectionTimeout, requestTimeout);
		}

		public void SetSession(LoginResponse loginResponse)
		{
			if (loginResponse == null)
			{
				throw new ArgumentNullException(nameof(loginResponse));
			}

			_currentUser = loginResponse;

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
			_currentUser = null;
			_disposed = true;
		}
	}
}