namespace backend.Business.Models;

/// <summary>
/// Represents an audit log entry for tracking critical system operations.
/// Records user actions, entity changes, and timestamps for compliance and debugging.
/// </summary>
public class AuditLog
{
	public int LogId { get; set; }
	public int? UserId { get; set; }
	public string Action { get; set; } = string.Empty;
	public string? EntityType { get; set; }
	public int? EntityId { get; set; }
	public string? Details { get; set; }
	public DateTime CreatedAt { get; set; }
}

