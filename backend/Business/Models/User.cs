namespace backend.Business.Models;

/// <summary>
/// Represents a user account in the system (Admin or Customer).
/// Stores user credentials, personal information, and account status.
/// </summary>
public class User
{
	public int UserId { get; set; }
	public string Username { get; set; } = string.Empty;
	public string PasswordHash { get; set; } = string.Empty;
	public string FullName { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string? PhoneNumber { get; set; }
	public string Role { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
	public bool IsActive { get; set; }
}

