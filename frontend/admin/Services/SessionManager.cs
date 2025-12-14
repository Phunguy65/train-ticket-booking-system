using sdk_client;
using sdk_client.Protocol;

namespace admin.Services;

/// <summary>
/// Manages user session state including authentication token and user information.
/// Singleton pattern ensures single source of truth for session data across the application.
/// </summary>
public sealed class SessionManager
{
	private static SessionManager? _instance;
	private static readonly object _lock = new object();

	/// <summary>
	/// Gets the singleton instance of SessionManager.
	/// </summary>
	public static SessionManager Instance
	{
		get
		{
			if (_instance == null)
			{
				lock (_lock)
				{
					_instance ??= new SessionManager();
				}
			}

			return _instance;
		}
	}

	private SessionManager()
	{
	}

	/// <summary>
	/// Gets or sets the API client instance for server communication.
	/// </summary>
	public ApiClient? ApiClient { get; private set; }

	/// <summary>
	/// Gets or sets the current session token.
	/// </summary>
	public string? SessionToken { get; set; }

	/// <summary>
	/// Gets or sets the current user ID.
	/// </summary>
	public int? UserId { get; set; }

	/// <summary>
	/// Gets or sets the current username.
	/// </summary>
	public string? Username { get; set; }

	/// <summary>
	/// Gets or sets the current user role.
	/// </summary>
	public string? Role { get; set; }

	/// <summary>
	/// Gets whether the user is currently authenticated.
	/// </summary>
	public bool IsAuthenticated => !string.IsNullOrEmpty(SessionToken);

	/// <summary>
	/// Initializes the API client with server connection details.
	/// </summary>
	public void Initialize(string host, int port, int connectionTimeout, int requestTimeout)
	{
		ApiClient = new ApiClient(host, port, connectionTimeout, requestTimeout);
	}

	/// <summary>
	/// Sets session data from login response.
	/// </summary>
	public void SetSession(LoginResponse loginResponse)
	{
		SessionToken = loginResponse.SessionToken;
		UserId = loginResponse.UserId;
		Username = loginResponse.Username;
		Role = loginResponse.Role;

		if (ApiClient != null)
		{
			ApiClient.SessionToken = loginResponse.SessionToken;
		}
	}

	/// <summary>
	/// Clears all session data (logout).
	/// </summary>
	public void ClearSession()
	{
		SessionToken = null;
		UserId = null;
		Username = null;
		Role = null;

		if (ApiClient != null)
		{
			ApiClient.SessionToken = null;
		}
	}
}