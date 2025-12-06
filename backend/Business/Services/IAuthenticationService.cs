using backend.Business.Models;

namespace backend.Business.Services;

/// <summary>
/// Service interface for user authentication operations.
/// Handles user registration, login, logout, and session management.
/// </summary>
public interface IAuthenticationService
{
	Task<(bool Success, string Message, int UserId)> RegisterAsync(string username, string password, string fullName,
		string email, string? phoneNumber);

	Task<(bool Success, string Message, string SessionToken, User? User)> LoginAsync(string username, string password);
	Task<bool> LogoutAsync(string sessionToken);
	Task<SessionInfo?> ValidateSessionAsync(string sessionToken);
}