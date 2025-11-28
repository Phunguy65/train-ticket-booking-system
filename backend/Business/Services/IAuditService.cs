using backend.Business.Models;

namespace backend.Business.Services;

/// <summary>
/// Service interface for audit logging operations.
/// Handles logging of critical system operations and retrieval of audit trails.
/// </summary>
public interface IAuditService
{
	Task LogAsync(int? userId, string action, string? entityType, int? entityId, string? details);
	Task<IEnumerable<AuditLog>> GetAuditLogsByUserIdAsync(int userId);
	Task<IEnumerable<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
	Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync();
}

