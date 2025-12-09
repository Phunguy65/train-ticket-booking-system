using backend.Business.Services;
using backend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace backend.Infrastructure.Services;

/// <summary>
/// Background service that periodically cleans up expired seat holds.
/// Runs on configurable interval to release seats from bookings with expired HoldExpiresAt timestamps.
/// Ensures seats become available again when users don't confirm their temporary holds.
/// Sends SignalR notifications to connected clients when seats are released.
/// </summary>
public class SeatHoldCleanupService : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<SeatHoldCleanupService> _logger;
	private readonly IConfiguration _configuration;
	private readonly IHubContext<BookingHub> _hubContext;
	private readonly int _cleanupIntervalSeconds;

	public SeatHoldCleanupService(
		IServiceProvider serviceProvider,
		ILogger<SeatHoldCleanupService> logger,
		IConfiguration configuration,
		IHubContext<BookingHub> hubContext)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
		_configuration = configuration;
		_hubContext = hubContext;
		_cleanupIntervalSeconds = _configuration.GetValue("Booking:HoldCleanupIntervalSeconds", 30);
	}

	/// <summary>
	/// Main execution loop that runs cleanup task on configured interval.
	/// Creates scoped service provider for each iteration to ensure proper dependency injection lifecycle.
	/// Logs cleanup results for monitoring and debugging.
	/// Sends SignalR notifications to clients when seats are released.
	/// </summary>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation(
			"Seat Hold Cleanup Service started. Cleanup interval: {IntervalSeconds} seconds",
			_cleanupIntervalSeconds);

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await Task.Delay(TimeSpan.FromSeconds(_cleanupIntervalSeconds), stoppingToken);

				// Create scope for scoped services (IBookingService, IUnitOfWork, etc.)
				using var scope = _serviceProvider.CreateScope();
				var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

				var (releasedCount, releasedSeatsByTrain) = await bookingService.CleanupExpiredHoldsAsync();

				if (releasedCount > 0)
				{
					_logger.LogInformation(
						"Seat Hold Cleanup: Released {ReleasedCount} expired holds at {Timestamp}",
						releasedCount, DateTime.UtcNow);

					// Send SignalR notifications to clients for each train
					foreach (var (trainId, seatIds) in releasedSeatsByTrain)
					{
						await NotifySeatReleasedAsync(trainId, seatIds);
					}
				}
			}
			catch (OperationCanceledException)
			{
				// Expected when service is stopping
				_logger.LogInformation("Seat Hold Cleanup Service is stopping.");
				break;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					"Error occurred during seat hold cleanup at {Timestamp}. Will retry on next interval.",
					DateTime.UtcNow);
				// Continue running despite errors
			}
		}

		_logger.LogInformation("Seat Hold Cleanup Service stopped.");
	}

	/// <summary>
	/// Sends SignalR notification to all clients in a train group when seats are released.
	/// Notifies clients that previously unavailable seats are now available for booking.
	/// </summary>
	private async Task NotifySeatReleasedAsync(int trainId, List<int> seatIds)
	{
		try
		{
			var groupName = $"train_{trainId}";
			await _hubContext.Clients.Group(groupName)
				.SendAsync("SeatReleased", new { TrainId = trainId, SeatIds = seatIds });

			_logger.LogInformation(
				"SignalR notification sent: {SeatCount} seats released for train {TrainId}",
				seatIds.Count, trainId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex,
				"Failed to send SignalR notification for train {TrainId}",
				trainId);
		}
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Seat Hold Cleanup Service is stopping gracefully...");
		await base.StopAsync(cancellationToken);
	}
}