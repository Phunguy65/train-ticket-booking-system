using backend.Business.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

namespace backend.Infrastructure.Security;

/// <summary>
/// Manages user sessions with token generation, validation, and timeout handling.
/// Stores active sessions in memory for fast authentication checks.
/// </summary>
public class SessionManager
{
	private readonly ConcurrentDictionary<string, SessionInfo> _sessions = new();
	private readonly int _sessionTimeoutMinutes;

	public SessionManager(IConfiguration configuration)
	{
		_sessionTimeoutMinutes = configuration.GetValue("Security:SessionTimeout", 30);
	}

	public string CreateSession(int userId, string username, string role)
	{
		var sessionToken = Guid.NewGuid().ToString();
		var sessionInfo = new SessionInfo
		{
			SessionToken = sessionToken,
			UserId = userId,
			Username = username,
			Role = role,
			CreatedAt = DateTime.UtcNow,
			ExpiresAt = DateTime.UtcNow.AddMinutes(_sessionTimeoutMinutes)
		};

		_sessions[sessionToken] = sessionInfo;
		return sessionToken;
	}

	public SessionInfo? GetSession(string sessionToken)
	{
		if (_sessions.TryGetValue(sessionToken, out var sessionInfo))
		{
			if (sessionInfo.ExpiresAt > DateTime.UtcNow)
			{
				sessionInfo.ExpiresAt = DateTime.UtcNow.AddMinutes(_sessionTimeoutMinutes);
				return sessionInfo;
			}
			else
			{
				_sessions.TryRemove(sessionToken, out _);
			}
		}

		return null;
	}

	public bool ValidateSession(string sessionToken)
	{
		return GetSession(sessionToken) != null;
	}

	public void RemoveSession(string sessionToken)
	{
		_sessions.TryRemove(sessionToken, out _);
	}

	public void CleanupExpiredSessions()
	{
		var expiredTokens = _sessions
			.Where(kvp => kvp.Value.ExpiresAt <= DateTime.UtcNow)
			.Select(kvp => kvp.Key)
			.ToList();

		foreach (var token in expiredTokens)
		{
			_sessions.TryRemove(token, out _);
		}
	}
}