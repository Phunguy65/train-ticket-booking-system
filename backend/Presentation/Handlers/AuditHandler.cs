using backend.Business.Services;
using backend.Presentation.Protocol;
using Newtonsoft.Json.Linq;

namespace backend.Presentation.Handlers;

/// <summary>
/// Handler for audit log commands (view audit logs).
/// Processes audit log requests with admin authorization and returns appropriate responses.
/// </summary>
public class AuditHandler
{
	private readonly IAuditService _auditService;
	private readonly IAuthenticationService _authenticationService;

	public AuditHandler(IAuditService auditService, IAuthenticationService authenticationService)
	{
		_auditService = auditService;
		_authenticationService = authenticationService;
	}

	public async Task<Response> HandleAsync(string action, JObject? data)
	{
		return action switch
		{
			"GetAuditLogs" => await HandleGetAuditLogsAsync(data),
			_ => new Response { Success = false, ErrorMessage = "Unknown audit action." }
		};
	}

	private async Task<Response> HandleGetAuditLogsAsync(JObject? data)
	{
		if (data == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid request data." };
		}

		var sessionToken = data["SessionToken"]?.Value<string>();
		if (string.IsNullOrEmpty(sessionToken))
		{
			return new Response { Success = false, ErrorMessage = "Session token is required." };
		}

		var session = await _authenticationService.ValidateSessionAsync(sessionToken);
		if (session == null || session.Role != "Admin")
		{
			return new Response { Success = false, ErrorMessage = "Admin access required." };
		}

		var request = data.ToObject<GetAuditLogsRequest>();
		if (request == null)
		{
			var allLogs = await _auditService.GetAllAuditLogsAsync();
			return new Response { Success = true, Data = allLogs };
		}

		if (request.UserId.HasValue)
		{
			var logs = await _auditService.GetAuditLogsByUserIdAsync(request.UserId.Value);
			return new Response { Success = true, Data = logs };
		}

		if (request.StartDate.HasValue && request.EndDate.HasValue)
		{
			var logs = await _auditService.GetAuditLogsByDateRangeAsync(request.StartDate.Value, request.EndDate.Value);
			return new Response { Success = true, Data = logs };
		}

		var allAuditLogs = await _auditService.GetAllAuditLogsAsync();
		return new Response { Success = true, Data = allAuditLogs };
	}
}

