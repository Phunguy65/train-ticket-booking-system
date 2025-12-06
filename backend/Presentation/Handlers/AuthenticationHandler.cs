using backend.Business.Services;
using backend.Presentation.Protocol;
using Newtonsoft.Json.Linq;

namespace backend.Presentation.Handlers;

/// <summary>
/// Handler for authentication-related commands (register, login, logout).
/// Processes authentication requests and returns appropriate responses.
/// </summary>
public class AuthenticationHandler
{
	private readonly IAuthenticationService _authenticationService;

	public AuthenticationHandler(IAuthenticationService authenticationService)
	{
		_authenticationService = authenticationService;
	}

	public async Task<Response> HandleAsync(string action, JObject? data)
	{
		return action switch
		{
			"Register" => await HandleRegisterAsync(data),
			"Login" => await HandleLoginAsync(data),
			"Logout" => await HandleLogoutAsync(data),
			_ => new Response { Success = false, ErrorMessage = "Unknown authentication action." }
		};
	}

	private async Task<Response> HandleRegisterAsync(JObject? data)
	{
		if (data == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid request data." };
		}

		var request = data.ToObject<RegisterRequest>();
		if (request == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid registration data." };
		}

		var result = await _authenticationService.RegisterAsync(
			request.Username, request.Password, request.FullName, request.Email, request.PhoneNumber);

		return new Response
		{
			Success = result.Success,
			ErrorMessage = result.Success ? null : result.Message,
			Data = result.Success ? new { UserId = result.UserId, Message = result.Message } : null
		};
	}

	private async Task<Response> HandleLoginAsync(JObject? data)
	{
		if (data == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid request data." };
		}

		var request = data.ToObject<LoginRequest>();
		if (request == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid login data." };
		}

		var result = await _authenticationService.LoginAsync(request.Username, request.Password);

		return new Response
		{
			Success = result.Success,
			ErrorMessage = result.Success ? null : result.Message,
			Data = result.Success
				? new LoginResponse
				{
					SessionToken = result.SessionToken,
					UserId = result.User!.UserId,
					Username = result.User.Username,
					Role = result.User.Role
				}
				: null
		};
	}

	private async Task<Response> HandleLogoutAsync(JObject? data)
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

		var success = await _authenticationService.LogoutAsync(request.SessionToken);

		return new Response
		{
			Success = success,
			ErrorMessage = success ? null : "Logout failed.",
			Data = success ? new { Message = "Logged out successfully." } : null
		};
	}
}