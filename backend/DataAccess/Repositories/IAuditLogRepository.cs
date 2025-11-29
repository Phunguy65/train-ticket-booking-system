using backend.Business.Models;

namespace backend.DataAccess.Repositories;

/// <summary>
/// Repository interface for AuditLog entity operations.
/// Provides methods for logging and retrieving audit trail records.
/// </summary>
public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(int logId);
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<AuditLog>> GetAllAsync();
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize);
    Task<int> CreateAsync(AuditLog auditLog);
}

