using backend.Business.Models;

namespace backend.DataAccess.Repositories;

/// <summary>
/// Repository interface for User entity operations.
/// Provides methods for user account management and authentication.
/// </summary>
public interface IUserRepository
{
	Task<User?> GetByIdAsync(int userId);
	Task<User?> GetByUsernameAsync(string username);
	Task<User?> GetByEmailAsync(string email);
	Task<IEnumerable<User>> GetAllAsync();
	Task<(IEnumerable<User> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize);
	Task<int> CreateAsync(User user);
	Task<bool> UpdateAsync(User user);
	Task<bool> DeleteAsync(int userId);
	Task<bool> UpdateIsActiveAsync(int userId, bool isActive);
}