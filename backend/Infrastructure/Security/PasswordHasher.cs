namespace backend.Infrastructure.Security;

/// <summary>
/// Provides password hashing and verification using BCrypt.
/// Ensures secure password storage by generating salted hashes.
/// </summary>
public class PasswordHasher
{
	public string HashPassword(string password)
	{
		return BCrypt.Net.BCrypt.HashPassword(password);
	}

	public bool VerifyPassword(string password, string passwordHash)
	{
		return BCrypt.Net.BCrypt.Verify(password, passwordHash);
	}
}