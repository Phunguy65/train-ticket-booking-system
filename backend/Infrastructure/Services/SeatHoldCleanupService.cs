using backend.Business.Services;

namespace backend.Infrastructure.Services;

/// <summary>
/// Background service that periodically cleans up expired seat holds.
/// Runs on configurable interval to release seats from bookings with expired HoldExpiresAt timestamps.
/// Ensures seats become available again when users don't confirm their temporary holds.
/// </summary>
public class SeatHoldCleanupService : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<SeatHoldCleanupService> _logger;
	private readonly IConfiguration _configuration;
	private readonly int _cleanupIntervalSeconds;

	public SeatHoldCleanupService(
		IServiceProvider serviceProvider,
		ILogger<SeatHoldCleanupService> logger,
		IConfiguration configuration)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
		_configuration = configuration;
		_cleanupIntervalSeconds = _configuration.GetValue("Booking:HoldCleanupIntervalSeconds", 30);
	}

	/// <summary>
	/// Main execution loop that runs cleanup task on configured interval.
	/// Creates scoped service provider for each iteration to ensure proper dependency injection lifecycle.
	/// Logs cleanup results for monitoring and debugging.
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

				int releasedCount = await bookingService.CleanupExpiredHoldsAsync();

				if (releasedCount > 0)
				{
					_logger.LogInformation(
						"Seat Hold Cleanup: Released {ReleasedCount} expired holds at {Timestamp}",
						releasedCount, DateTime.UtcNow);
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

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Seat Hold Cleanup Service is stopping gracefully...");
		await base.StopAsync(cancellationToken);
	}
}