using Microsoft.AspNetCore.SignalR;

namespace backend.Hubs;

/// <summary>
/// SignalR Hub for real-time booking notifications.
/// Manages client connections and broadcasts seat availability updates to subscribed clients.
/// </summary>
public class BookingHub : Hub
{
	private readonly ILogger<BookingHub> _logger;

	public BookingHub(ILogger<BookingHub> logger)
	{
		_logger = logger;
	}

	public async Task JoinTrainGroup(int trainId)
	{
		var groupName = $"train_{trainId}";
		await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
		_logger.LogInformation("Client {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
	}

	public async Task LeaveTrainGroup(int trainId)
	{
		var groupName = $"train_{trainId}";
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
		_logger.LogInformation("Client {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
	}

	public override async Task OnConnectedAsync()
	{
		_logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
		await base.OnConnectedAsync();
	}

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		_logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
		await base.OnDisconnectedAsync(exception);
	}
}