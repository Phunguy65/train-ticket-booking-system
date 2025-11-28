using backend.Business.Models;

namespace backend.Business.Services;

/// <summary>
/// Service interface for user management operations.
/// Handles user profile updates and admin user management functions.
/// </summary>
public interface IUserService
{
	Task<User?> GetUserByIdAsync(int userId);
	Task<IEnumerable<User>> GetAllUsersAsync();
	Task<(bool Success, string Message)> UpdateUserProfileAsync(int userId, string? fullName, string? email, string? phoneNumber);
	Task<(bool Success, string Message)> LockUnlockUserAsync(int userId, bool isActive);
}

