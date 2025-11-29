using backend.Business.Models;
using backend.DataAccess.DbContext;
using Dapper;

namespace backend.DataAccess.Repositories;

/// <summary>
/// Repository implementation for AuditLog entity operations using Dapper.
/// Handles logging and retrieval of audit trail records for compliance and debugging.
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly DapperContext _context;

    public AuditLogRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<AuditLog?> GetByIdAsync(int logId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM AuditLog WHERE LogId = @LogId";
        return await connection.QueryFirstOrDefaultAsync<AuditLog>(sql, new { LogId = logId });
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM AuditLog WHERE UserId = @UserId ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<AuditLog>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM AuditLog WHERE CreatedAt BETWEEN @StartDate AND @EndDate ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<AuditLog>(sql, new { StartDate = startDate, EndDate = endDate });
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM AuditLog ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<AuditLog>(sql);
    }

    public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize)
    {
        using var connection = _context.CreateConnection();

        var countSql = "SELECT COUNT(*) FROM AuditLog";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);

        var offset = (pageNumber - 1) * pageSize;
        var dataSql = @"
			SELECT * FROM AuditLog
			ORDER BY CreatedAt DESC
			OFFSET @Offset ROWS
			FETCH NEXT @PageSize ROWS ONLY";

        var items = await connection.QueryAsync<AuditLog>(dataSql, new { Offset = offset, PageSize = pageSize });

        return (items, totalCount);
    }

    public async Task<int> CreateAsync(AuditLog auditLog)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
			INSERT INTO AuditLog (UserId, [Action], EntityType, EntityId, Details)
			VALUES (@UserId, @Action, @EntityType, @EntityId, @Details);
			SELECT CAST(SCOPE_IDENTITY() as int);";
        return await connection.ExecuteScalarAsync<int>(sql, auditLog);
    }
}

