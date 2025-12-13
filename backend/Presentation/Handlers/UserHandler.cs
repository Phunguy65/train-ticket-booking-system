using backend.Business.Services;
using backend.Presentation.Protocol;
using Newtonsoft.Json.Linq;

namespace backend.Presentation.Handlers;

/// <summary>
/// Handler for user management commands (profile updates, lock/unlock accounts).
/// Processes user-related requests with session validation and returns appropriate responses.
/// </summary>
public class UserHandler
{
	private readonly IUserService _userService;
	private readonly IAuthenticationService _authenticationService;

	public UserHandler(IUserService userService, IAuthenticationService authenticationService)
	{
		_userService = userService;
		_authenticationService = authenticationService;
	}

	public async Task<Response> HandleAsync(string action, JObject? data)
	{
		return action switch
		{
			"GetAllUsers" => await HandleGetAllUsersAsync(data),
			"GetCurrentUser" => await HandleGetCurrentUserAsync(data),
			"UpdateUserProfile" => await HandleUpdateUserProfileAsync(data),
			"LockUnlockUser" => await HandleLockUnlockUserAsync(data),
			_ => new Response { Success = false, ErrorMessage = "Unknown user action." }
		};
	}

	private async Task<Response> HandleGetAllUsersAsync(JObject? data)
	{
		if (data == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid request data." };
		}

		var request = data.ToObject<AuthenticatedRequest>();
		if (request == null || string.IsNullOrEmpty(request.SessionToken))
		{
			return new Response { Success = false, ErrorMessage = "Session token is required." };
		}

		var session = await _authenticationService.ValidateSessionAsync(request.SessionToken);
		if (session == null || session.Role != "Admin")
		{
			return new Response { Success = false, ErrorMessage = "Admin access required." };
		}

		var pageNumber = data["PageNumber"]?.Value<int>();
		var pageSize = data["PageSize"]?.Value<int>();

		if (pageNumber.HasValue && pageSize.HasValue)
		{
			if (pageNumber.Value < 1 || pageSize.Value < 1 || pageSize.Value > 100)
			{
				return new Response
				{
					Success = false,
					ErrorMessage =
						"Invalid pagination parameters. PageNumber must be >= 1, PageSize must be between 1 and 100."
				};
			}

			var pagedUsers = await _userService.GetAllUsersAsync(pageNumber.Value, pageSize.Value);
			return new Response { Success = true, Data = pagedUsers };
		}

		var users = await _userService.GetAllUsersAsync();
		return new Response { Success = true, Data = users };
	}

	private async Task<Response> HandleGetCurrentUserAsync(JObject? data)
	{
		if (data == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid request data." };
		}

		var request = data.ToObject<AuthenticatedRequest>();
		if (request == null || string.IsNullOrEmpty(request.SessionToken))
		{
			return new Response { Success = false, ErrorMessage = "Session token is required." };
		}

		var session = await _authenticationService.ValidateSessionAsync(request.SessionToken);
		if (session == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid or expired session." };
		}

		var user = await _userService.GetUserByIdAsync(session.UserId);
		if (user == null)
		{
			return new Response { Success = false, ErrorMessage = "User not found." };
		}

		// Return user profile without password hash
		return new Response
		{
			Success = true,
			Data = new
			{
				user.UserId,
				user.Username,
				user.FullName,
				user.Email,
				user.PhoneNumber,
				user.Role,
				user.CreatedAt,
				user.IsActive
			}
		};
	}

	private async Task<Response> HandleUpdateUserProfileAsync(JObject? data)
	{
		if (data == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid request data." };
		}

		var request = data.ToObject<UpdateUserRequest>();
		if (request == null || string.IsNullOrEmpty(request.SessionToken))
		{
			return new Response { Success = false, ErrorMessage = "Session token is required." };
		}

		var session = await _authenticationService.ValidateSessionAsync(request.SessionToken);
		if (session == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid or expired session." };
		}

		var result =
			await _userService.UpdateUserProfileAsync(session.UserId, request.FullName, request.Email,
				request.PhoneNumber);
		return new Response
		{
			Success = result.Success,
			ErrorMessage = result.Success ? null : result.Message,
			Data = result.Success ? new { Message = result.Message } : null
		};
	}

	private async Task<Response> HandleLockUnlockUserAsync(JObject? data)
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

		var request = data.ToObject<LockUnlockUserRequest>();
		if (request == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid request data." };
		}

		var result = await _userService.LockUnlockUserAsync(request.UserId, request.IsActive);
		return new Response
		{
			Success = result.Success,
			ErrorMessage = result.Success ? null : result.Message,
			Data = result.Success ? new { Message = result.Message } : null
		};
	}
}