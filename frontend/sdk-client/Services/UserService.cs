using sdk_client.Protocol;
using System.Threading.Tasks;

namespace sdk_client.Services
{
	/// <summary>
	/// Service for user management operations including profile updates and account management.
	/// Provides methods for managing user accounts and profiles.
	/// </summary>
	public class UserService
	{
		private readonly ApiClient _apiClient;

		/// <summary>
		/// Initializes a new instance of UserService with an API client.
		/// </summary>
		/// <param name="apiClient">API client for server communication</param>
		public UserService(ApiClient apiClient)
		{
			_apiClient = apiClient;
		}

		/// <summary>
		/// Retrieves all users in the system with optional pagination.
		/// Requires admin privileges.
		/// </summary>
		/// <param name="pageNumber">Page number (1-based)</param>
		/// <param name="pageSize">Number of items per page (1-100)</param>
		/// <returns>List of all users or paginated result</returns>
		public async Task<object?> GetAllUsersAsync(int? pageNumber = null, int? pageSize = null)
		{
			object? requestData = null;

			if (pageNumber.HasValue && pageSize.HasValue)
			{
				requestData = new { PageNumber = pageNumber.Value, PageSize = pageSize.Value };
			}

			var response = await _apiClient.SendRequestAsync("User.GetAllUsers", requestData).ConfigureAwait(false);
			return response.Data;
		}

		/// <summary>
		/// Updates the profile information for the current authenticated user.
		/// Requires an active session token for authentication.
		/// </summary>
		/// <param name="fullName">Updated full name (optional)</param>
		/// <param name="email">Updated email address (optional)</param>
		/// <param name="phoneNumber">Updated phone number (optional)</param>
		/// <returns>Response indicating update success</returns>
		public async Task<Response> UpdateUserProfileAsync(string? fullName, string? email, string? phoneNumber)
		{
			var request = new UpdateUserRequest { FullName = fullName, Email = email, PhoneNumber = phoneNumber };

			return await _apiClient.SendRequestAsync("User.UpdateUserProfile", request).ConfigureAwait(false);
		}

		/// <summary>
		/// Locks or unlocks a user account.
		/// Requires admin privileges.
		/// </summary>
		/// <param name="userId">Unique user identifier</param>
		/// <param name="isActive">True to unlock/activate, false to lock/deactivate</param>
		/// <returns>Response indicating lock/unlock success</returns>
		public async Task<Response> LockUnlockUserAsync(int userId, bool isActive)
		{
			var request = new LockUnlockUserRequest { UserId = userId, IsActive = isActive };

			return await _apiClient.SendRequestAsync("User.LockUnlockUser", request).ConfigureAwait(false);
		}
	}
}