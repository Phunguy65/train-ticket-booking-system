namespace backend.Business.Models;

/// <summary>
/// Represents an active user session with authentication token.
/// Used for session management and user authentication tracking.
/// </summary>
public class SessionInfo
{
	public string SessionToken { get; set; } = string.Empty;
	public int UserId { get; set; }
	public string Username { get; set; } = string.Empty;
	public string Role { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
	public DateTime ExpiresAt { get; set; }
}

