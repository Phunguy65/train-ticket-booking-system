using backend.Business.Models;
using backend.DataAccess.Repositories;
using backend.Infrastructure.Security;

namespace backend.Business.Services;

/// <summary>
/// Service implementation for user authentication operations.
/// Handles user registration with password hashing, login with session creation, and logout.
/// </summary>
public class AuthenticationService(
	IUserRepository userRepository,
	PasswordHasher passwordHasher,
	SessionManager sessionManager,
	IAuditService auditService)
	: IAuthenticationService
{
	public async Task<(bool Success, string Message, int UserId)> RegisterAsync(
		string username, string password, string fullName, string email, string? phoneNumber)
	{
		var existingUser = await userRepository.GetByUsernameAsync(username);
		if (existingUser != null)
		{
			return (false, "Username already exists.", 0);
		}

		var existingEmail = await userRepository.GetByEmailAsync(email);
		if (existingEmail != null)
		{
			return (false, "Email already exists.", 0);
		}

		var user = new User
		{
			Username = username,
			PasswordHash = passwordHasher.HashPassword(password),
			FullName = fullName,
			Email = email,
			PhoneNumber = phoneNumber,
			Role = "Customer",
			IsActive = true
		};

		var userId = await userRepository.CreateAsync(user);
		await auditService.LogAsync(userId, "User Registration", "User", userId,
			$"User {username} registered successfully.");

		return (true, "Registration successful.", userId);
	}

	public async Task<(bool Success, string Message, string SessionToken, User? User)> LoginAsync(string username,
		string password)
	{
		var user = await userRepository.GetByUsernameAsync(username);
		if (user == null)
		{
			return (false, "Invalid username or password.", string.Empty, null);
		}

		if (!user.IsActive)
		{
			return (false, "Account is locked. Please contact administrator.", string.Empty, null);
		}

		if (!passwordHasher.VerifyPassword(password, user.PasswordHash))
		{
			return (false, "Invalid username or password.", string.Empty, null);
		}

		var sessionToken = sessionManager.CreateSession(user.UserId, user.Username, user.Role);
		await auditService.LogAsync(user.UserId, "User Login", "User", user.UserId,
			$"User {username} logged in successfully.");

		return (true, "Login successful.", sessionToken, user);
	}

	public async Task<bool> LogoutAsync(string sessionToken)
	{
		var session = sessionManager.GetSession(sessionToken);
		if (session != null)
		{
			sessionManager.RemoveSession(sessionToken);
			await auditService.LogAsync(session.UserId, "User Logout", "User", session.UserId,
				$"User {session.Username} logged out.");
			return true;
		}

		return false;
	}

	public async Task<SessionInfo?> ValidateSessionAsync(string sessionToken)
	{
		return await Task.FromResult(sessionManager.GetSession(sessionToken));
	}
}