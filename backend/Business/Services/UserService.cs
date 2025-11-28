using backend.Business.Models;
using backend.DataAccess.Repositories;

namespace backend.Business.Services;

/// <summary>
/// Service implementation for user management operations.
/// Handles user profile updates and admin user management functions.
/// </summary>
public class UserService : IUserService
{
	private readonly IUserRepository _userRepository;
	private readonly IAuditService _auditService;

	public UserService(IUserRepository userRepository, IAuditService auditService)
	{
		_userRepository = userRepository;
		_auditService = auditService;
	}

	public async Task<User?> GetUserByIdAsync(int userId)
	{
		return await _userRepository.GetByIdAsync(userId);
	}

	public async Task<IEnumerable<User>> GetAllUsersAsync()
	{
		return await _userRepository.GetAllAsync();
	}

	public async Task<(bool Success, string Message)> UpdateUserProfileAsync(int userId, string? fullName, string? email, string? phoneNumber)
	{
		var user = await _userRepository.GetByIdAsync(userId);
		if (user == null)
		{
			return (false, "User not found.");
		}

		if (!string.IsNullOrEmpty(fullName))
		{
			user.FullName = fullName;
		}

		if (!string.IsNullOrEmpty(email))
		{
			var existingEmail = await _userRepository.GetByEmailAsync(email);
			if (existingEmail != null && existingEmail.UserId != userId)
			{
				return (false, "Email already exists.");
			}
			user.Email = email;
		}

		if (phoneNumber != null)
		{
			user.PhoneNumber = phoneNumber;
		}

		var success = await _userRepository.UpdateAsync(user);
		if (success)
		{
			await _auditService.LogAsync(userId, "User Profile Updated", "User", userId, $"User {user.Username} updated profile.");
			return (true, "Profile updated successfully.");
		}

		return (false, "Failed to update profile.");
	}

	public async Task<(bool Success, string Message)> LockUnlockUserAsync(int userId, bool isActive)
	{
		var user = await _userRepository.GetByIdAsync(userId);
		if (user == null)
		{
			return (false, "User not found.");
		}

		var success = await _userRepository.UpdateIsActiveAsync(userId, isActive);
		if (success)
		{
			var action = isActive ? "unlocked" : "locked";
			await _auditService.LogAsync(null, $"User Account {action}", "User", userId, $"User {user.Username} account {action}.");
			return (true, $"User account {action} successfully.");
		}

		return (false, "Failed to update user status.");
	}
}

