using backend.Business.Models;
using backend.DataAccess.DbContext;
using Dapper;

namespace backend.DataAccess.Repositories;

/// <summary>
/// Repository implementation for User entity operations using Dapper.
/// Handles user account management and authentication queries.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly DapperContext _context;

    public UserRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM [User] WHERE UserId = @UserId";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM [User] WHERE Username = @Username";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM [User] WHERE Email = @Email";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM [User] ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<User>(sql);
    }

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize)
    {
        using var connection = _context.CreateConnection();

        var countSql = "SELECT COUNT(*) FROM [User]";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);

        var offset = (pageNumber - 1) * pageSize;
        var dataSql = @"
			SELECT * FROM [User]
			ORDER BY CreatedAt DESC
			OFFSET @Offset ROWS
			FETCH NEXT @PageSize ROWS ONLY";

        var items = await connection.QueryAsync<User>(dataSql, new { Offset = offset, PageSize = pageSize });

        return (items, totalCount);
    }

    public async Task<int> CreateAsync(User user)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
			INSERT INTO [User] (Username, PasswordHash, FullName, Email, PhoneNumber, [Role], IsActive)
			VALUES (@Username, @PasswordHash, @FullName, @Email, @PhoneNumber, @Role, @IsActive);
			SELECT CAST(SCOPE_IDENTITY() as int);";
        return await connection.ExecuteScalarAsync<int>(sql, user);
    }

    public async Task<bool> UpdateAsync(User user)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
			UPDATE [User]
			SET FullName = @FullName, Email = @Email, PhoneNumber = @PhoneNumber, IsActive = @IsActive
			WHERE UserId = @UserId";
        var rowsAffected = await connection.ExecuteAsync(sql, user);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int userId)
    {
        using var connection = _context.CreateConnection();
        var sql = "DELETE FROM [User] WHERE UserId = @UserId";
        var rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId });
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateIsActiveAsync(int userId, bool isActive)
    {
        using var connection = _context.CreateConnection();
        var sql = "UPDATE [User] SET IsActive = @IsActive WHERE UserId = @UserId";
        var rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId, IsActive = isActive });
        return rowsAffected > 0;
    }
}

