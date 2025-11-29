using backend.Business.Models;
using backend.DataAccess.Repositories;

namespace backend.Business.Services;

/// <summary>
/// Service implementation for audit logging operations.
/// Handles logging of critical system operations for compliance and debugging purposes.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task LogAsync(int? userId, string action, string? entityType, int? entityId, string? details)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details
        };

        await _auditLogRepository.CreateAsync(auditLog);
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByUserIdAsync(int userId)
    {
        return await _auditLogRepository.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _auditLogRepository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync()
    {
        return await _auditLogRepository.GetAllAsync();
    }

    public async Task<PagedResult<AuditLog>> GetAllAuditLogsAsync(int pageNumber, int pageSize)
    {
        var (items, totalCount) = await _auditLogRepository.GetAllAsync(pageNumber, pageSize);
        return new PagedResult<AuditLog>
        {
            Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize
        };
    }
}

