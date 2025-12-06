using System.Net.Sockets;
using System.Text;
using backend.Presentation.Handlers;
using backend.Presentation.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace backend.Presentation;

/// <summary>
/// Handles individual client connections and processes incoming requests.
/// Routes requests to appropriate handlers and sends responses back to clients.
/// </summary>
public class ClientHandler(
	TcpClient client,
	AuthenticationHandler authenticationHandler,
	TrainHandler trainHandler,
	BookingHandler bookingHandler,
	UserHandler userHandler,
	AuditHandler auditHandler,
	ILogger<ClientHandler> logger)
{
	public async Task HandleAsync()
	{
		try
		{
			await using var stream = client.GetStream();
			var buffer = new byte[8192];

			while (client.Connected)
			{
				var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
				if (bytesRead == 0) break;

				var requestJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
				logger.LogInformation("Received request: {Request}", requestJson);

				var response = await ProcessRequestAsync(requestJson);
				var responseJson = JsonConvert.SerializeObject(response);
				var responseBytes = Encoding.UTF8.GetBytes(responseJson);

				await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
				logger.LogInformation("Sent response: {Response}", responseJson);
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error handling client connection.");
		}
		finally
		{
			client.Close();
		}
	}

	private async Task<Response> ProcessRequestAsync(string requestJson)
	{
		try
		{
			var request = JsonConvert.DeserializeObject<Request>(requestJson);
			if (request == null || string.IsNullOrEmpty(request.Action))
			{
				return new Response
				{
					Success = false, ErrorMessage = "Invalid request format.", RequestId = string.Empty
				};
			}

			var data = request.Data != null ? JObject.FromObject(request.Data) : null;
			var response = await RouteRequestAsync(request.Action, data);
			response.RequestId = request.RequestId;

			return response;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error processing request.");
			return new Response
			{
				Success = false, ErrorMessage = $"Server error: {ex.Message}", RequestId = string.Empty
			};
		}
	}

	private async Task<Response> RouteRequestAsync(string action, JObject? data)
	{
		var parts = action.Split('.');
		if (parts.Length != 2)
		{
			return new Response
			{
				Success = false, ErrorMessage = "Invalid action format. Expected 'Category.Action'."
			};
		}

		var category = parts[0];
		var subAction = parts[1];

		return category switch
		{
			"Authentication" => await authenticationHandler.HandleAsync(subAction, data),
			"Train" => await trainHandler.HandleAsync(subAction, data),
			"Booking" => await bookingHandler.HandleAsync(subAction, data),
			"User" => await userHandler.HandleAsync(subAction, data),
			"Audit" => await auditHandler.HandleAsync(subAction, data),
			_ => new Response { Success = false, ErrorMessage = $"Unknown category: {category}" }
		};
	}
}