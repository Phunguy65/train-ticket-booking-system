using sdk_client.Protocol;
using System.Threading.Tasks;

namespace sdk_client.Services
{
	/// <summary>
	/// Service for user authentication operations including registration, login, and logout.
	/// Manages user session tokens and authentication state.
	/// </summary>
	public class AuthenticationService
	{
		private readonly ApiClient _apiClient;

		/// <summary>
		/// Initializes a new instance of AuthenticationService with an API client.
		/// </summary>
		/// <param name="apiClient">API client for server communication</param>
		public AuthenticationService(ApiClient apiClient)
		{
			_apiClient = apiClient;
		}

		/// <summary>
		/// Registers a new user account.
		/// </summary>
		/// <param name="username">Unique username for the account</param>
		/// <param name="password">Account password</param>
		/// <param name="fullName">User's full name</param>
		/// <param name="email">User's email address</param>
		/// <param name="phoneNumber">User's phone number (optional)</param>
		/// <returns>Response containing registration result</returns>
		public async Task<Response> RegisterAsync(string username, string password, string fullName, string email,
			string phoneNumber = null)
		{
			var request = new RegisterRequest
			{
				Username = username,
				Password = password,
				FullName = fullName,
				Email = email,
				PhoneNumber = phoneNumber
			};

			return await _apiClient.SendRequestAsync("Authentication.Register", request).ConfigureAwait(false);
		}

		/// <summary>
		/// Authenticates a user and establishes a session.
		/// Automatically stores the session token in the API client for subsequent authenticated requests.
		/// </summary>
		/// <param name="username">Username for authentication</param>
		/// <param name="password">Password for authentication</param>
		/// <returns>LoginResponse containing session token and user information</returns>
		public async Task<LoginResponse> LoginAsync(string username, string password)
		{
			var request = new LoginRequest { Username = username, Password = password };

			var loginResponse = await _apiClient.SendRequestAsync<LoginResponse>("Authentication.Login", request)
				.ConfigureAwait(false);

			if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.SessionToken))
			{
				_apiClient.SessionToken = loginResponse.SessionToken;
			}

			return loginResponse;
		}

		/// <summary>
		/// Logs out the current user and invalidates the session.
		/// Clears the session token from the API client.
		/// </summary>
		/// <returns>Response indicating logout success</returns>
		public async Task<Response> LogoutAsync()
		{
			var response = await _apiClient.SendRequestAsync("Authentication.Logout").ConfigureAwait(false);

			_apiClient.SessionToken = null;

			return response;
		}
	}
}