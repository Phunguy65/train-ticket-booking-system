using backend.Business.Models;
using backend.DataAccess.Repositories;
using backend.DataAccess.UnitOfWork;
using backend.Presentation.Protocol;

namespace backend.Business.Services;

/// <summary>
/// Service implementation for booking operations.
/// Handles ticket booking with pessimistic locking to prevent race conditions on seat availability.
/// </summary>
public class BookingService : IBookingService
{
	private readonly IBookingRepository _bookingRepository;
	private readonly ISeatRepository _seatRepository;
	private readonly ITrainRepository _trainRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IAuditService _auditService;
	private readonly IConfiguration _configuration;

	public BookingService(
		IBookingRepository bookingRepository,
		ISeatRepository seatRepository,
		ITrainRepository trainRepository,
		IUnitOfWork unitOfWork,
		IAuditService auditService,
		IConfiguration configuration)
	{
		_bookingRepository = bookingRepository;
		_seatRepository = seatRepository;
		_trainRepository = trainRepository;
		_unitOfWork = unitOfWork;
		_auditService = auditService;
		_configuration = configuration;
	}

	public async Task<IEnumerable<Seat>> GetSeatMapAsync(int trainId)
	{
		return await _seatRepository.GetByTrainIdAsync(trainId);
	}

	public async Task<(bool Success, string Message, int BookingId)> BookTicketAsync(int userId, int trainId,
		int seatId)
	{
		try
		{
			_unitOfWork.BeginTransaction();

			var train = await _trainRepository.GetByIdAsync(trainId);
			if (train == null)
			{
				_unitOfWork.Rollback();
				return (false, "Train not found.", 0);
			}

			if (train.Status != "Active")
			{
				_unitOfWork.Rollback();
				return (false, "Train is not available for booking.", 0);
			}

			var seat = await _seatRepository.GetByIdWithLockAsync(seatId);
			if (seat == null)
			{
				_unitOfWork.Rollback();
				return (false, "Seat not found.", 0);
			}

			if (seat.TrainId != trainId)
			{
				_unitOfWork.Rollback();
				return (false, "Seat does not belong to this train.", 0);
			}

			if (!seat.IsAvailable)
			{
				_unitOfWork.Rollback();
				return (false, "Seat is already booked.", 0);
			}

			var booking = new Booking
			{
				UserId = userId,
				TrainId = trainId,
				SeatId = seatId,
				BookingStatus = "Confirmed",
				TotalAmount = train.TicketPrice,
				PaymentStatus = "Paid"
			};

			var bookingId = await _bookingRepository.CreateAsync(booking);
			await _seatRepository.UpdateAvailabilityAsync(seatId, false);

			_unitOfWork.Commit();

			await _auditService.LogAsync(userId, "Ticket Booked", "Booking", bookingId,
				$"User booked seat {seat.SeatNumber} on train {train.TrainNumber}.");

			return (true, "Ticket booked successfully.", bookingId);
		}
		catch (Exception ex)
		{
			_unitOfWork.Rollback();
			return (false, $"Booking failed: {ex.Message}", 0);
		}
	}

	public async Task<(bool Success, string Message, List<int> BookingIds)> BookMultipleTicketsAsync(int userId,
		int trainId, List<int> seatIds)
	{
		const int maxSeatsPerBooking = 10;

		if (seatIds.Count == 0)
		{
			return (false, "No seats selected.", new List<int>());
		}

		if (seatIds.Count > maxSeatsPerBooking)
		{
			return (false, $"Cannot book more than {maxSeatsPerBooking} seats at once.", new List<int>());
		}

		if (seatIds.Distinct().Count() != seatIds.Count)
		{
			return (false, "Duplicate seat IDs detected.", new List<int>());
		}

		try
		{
			_unitOfWork.BeginTransaction();

			var train = await _trainRepository.GetByIdAsync(trainId);
			if (train == null)
			{
				_unitOfWork.Rollback();
				return (false, "Train not found.", new List<int>());
			}

			if (train.Status != "Active")
			{
				_unitOfWork.Rollback();
				return (false, "Train is not available for booking.", new List<int>());
			}

			var seats = (await _seatRepository.GetMultipleSeatsWithLockAsync(seatIds)).ToList();

			if (seats.Count != seatIds.Count)
			{
				_unitOfWork.Rollback();
				return (false, "One or more seats not found.", new List<int>());
			}

			if (seats.Any(s => s.TrainId != trainId))
			{
				_unitOfWork.Rollback();
				return (false, "One or more seats do not belong to this train.", new List<int>());
			}

			var unavailableSeats = seats.Where(s => !s.IsAvailable).Select(s => s.SeatNumber).ToList();
			if (unavailableSeats.Any())
			{
				_unitOfWork.Rollback();
				return (false, $"Seats already booked: {string.Join(", ", unavailableSeats)}", new List<int>());
			}

			var totalAmount = train.TicketPrice * seatIds.Count;

			// Create all bookings in a single batch operation for better performance
			var bookingsToCreate = seats.Select(seat => new Booking
			{
				UserId = userId,
				TrainId = trainId,
				SeatId = seat.SeatId,
				BookingStatus = "Confirmed",
				TotalAmount = train.TicketPrice,
				PaymentStatus = "Paid"
			}).ToList();

			var bookingIds = await _bookingRepository.CreateBatchAsync(bookingsToCreate);

			await _seatRepository.UpdateMultipleSeatsAvailabilityAsync(seatIds, false);

			_unitOfWork.Commit();

			var seatNumbers = string.Join(", ", seats.Select(s => s.SeatNumber));
			await _auditService.LogAsync(userId, "Multiple Tickets Booked", "Booking", bookingIds.First(),
				$"User booked {seatIds.Count} seats ({seatNumbers}) on train {train.TrainNumber}. Total: {totalAmount:C}");

			return (true, $"Successfully booked {seatIds.Count} tickets.", bookingIds);
		}
		catch (Exception ex)
		{
			_unitOfWork.Rollback();
			return (false, $"Booking failed: {ex.Message}", new List<int>());
		}
	}

	public async Task<(bool Success, string Message)> CancelBookingAsync(int bookingId, int userId, bool isAdmin)
	{
		var booking = await _bookingRepository.GetByIdAsync(bookingId);
		if (booking == null)
		{
			return (false, "Booking not found.");
		}

		if (!isAdmin && booking.UserId != userId)
		{
			return (false, "You are not authorized to cancel this booking.");
		}

		if (booking.BookingStatus == "Cancelled")
		{
			return (false, "Booking is already cancelled.");
		}

		try
		{
			_unitOfWork.BeginTransaction();

			await _bookingRepository.CancelBookingAsync(bookingId);
			await _seatRepository.UpdateAvailabilityAsync(booking.SeatId, true);

			_unitOfWork.Commit();

			await _auditService.LogAsync(userId, "Booking Cancelled", "Booking", bookingId,
				$"Booking {bookingId} cancelled by {(isAdmin ? "admin" : "user")}.");

			return (true, "Booking cancelled successfully.");
		}
		catch (Exception ex)
		{
			_unitOfWork.Rollback();
			return (false, $"Cancellation failed: {ex.Message}");
		}
	}

	public async Task<IEnumerable<Booking>> GetBookingHistoryAsync(int userId)
	{
		return await _bookingRepository.GetByUserIdAsync(userId);
	}

	public async Task<List<BookingHistory>> GetBookingHistoryDetailedAsync(int userId)
	{
		return await _bookingRepository.GetBookingHistoryAsync(userId);
	}

	public async Task<PagedResult<BookingHistory>> GetBookingHistoryDetailedAsync(int userId, int pageNumber,
		int pageSize)
	{
		var (items, totalCount) = await _bookingRepository.GetBookingHistoryAsync(userId, pageNumber, pageSize);
		return new PagedResult<BookingHistory>
		{
			Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize
		};
	}

	public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
	{
		return await _bookingRepository.GetAllAsync();
	}

	public async Task<PagedResult<Booking>> GetAllBookingsAsync(int pageNumber, int pageSize)
	{
		var (items, totalCount) = await _bookingRepository.GetAllAsync(pageNumber, pageSize);
		return new PagedResult<Booking>
		{
			Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize
		};
	}

	public async Task<Booking?> GetBookingByIdAsync(int bookingId)
	{
		return await _bookingRepository.GetByIdAsync(bookingId);
	}

	/// <summary>
	/// Temporarily holds seats for a user with configurable timeout.
	/// Creates bookings with Pending status and HoldExpiresAt timestamp.
	/// Validates seat availability, train status, and max holds per user limit.
	/// Returns booking IDs and expiration time in UTC.
	/// </summary>
	public async Task<(bool Success, string Message, List<int> BookingIds, DateTime ExpiresAt)> HoldSeatsAsync(
		int userId, int trainId, List<int> seatIds)
	{
		if (seatIds.Count == 0)
		{
			return (false, "No seats selected.", new List<int>(), DateTime.MinValue);
		}

		try
		{
			_unitOfWork.BeginTransaction();

			// Check max holds per user limit
			int maxHoldsPerUser = _configuration.GetValue("Booking:MaxHoldsPerUser", 10);
			List<Booking> activeHolds = await _bookingRepository.GetUserActiveHoldsAsync(userId);
			if (activeHolds.Count + seatIds.Count > maxHoldsPerUser)
			{
				_unitOfWork.Rollback();
				return (false, $"Maximum {maxHoldsPerUser} active holds allowed per user.", new List<int>(),
					DateTime.MinValue);
			}

			// Validate train exists and is available
			Train? train = await _trainRepository.GetByIdAsync(trainId);
			if (train == null)
			{
				_unitOfWork.Rollback();
				return (false, "Train not found.", new List<int>(), DateTime.MinValue);
			}

			if (train.Status != "Active")
			{
				_unitOfWork.Rollback();
				return (false, "Train is not available for booking.", new List<int>(), DateTime.MinValue);
			}

			// Lock and validate all seats
			List<Seat> seats = (await _seatRepository.GetMultipleSeatsWithLockAsync(seatIds)).ToList();
			if (seats.Count != seatIds.Count)
			{
				_unitOfWork.Rollback();
				return (false, "One or more seats not found.", new List<int>(), DateTime.MinValue);
			}

			List<Seat> unavailableSeats = seats.Where(s => !s.IsAvailable).ToList();
			if (unavailableSeats.Any())
			{
				_unitOfWork.Rollback();
				string seatNumbers = string.Join(", ", unavailableSeats.Select(s => s.SeatNumber));
				return (false, $"Seats already booked: {seatNumbers}", new List<int>(), DateTime.MinValue);
			}

			// Calculate hold expiration time in UTC
			int holdTimeoutMinutes = _configuration.GetValue("Booking:SeatHoldTimeoutMinutes", 5);
			DateTime holdExpiresAt = DateTime.UtcNow.AddMinutes(holdTimeoutMinutes);

			// Create pending bookings with hold expiration
			List<Booking> bookings = seats.Select(seat => new Booking
			{
				UserId = userId,
				TrainId = trainId,
				SeatId = seat.SeatId,
				BookingStatus = "Pending",
				TotalAmount = train.TicketPrice,
				PaymentStatus = "Pending"
			}).ToList();

			List<int> bookingIds = await _bookingRepository.CreateBatchWithHoldAsync(bookings, holdExpiresAt);

			// Mark seats as unavailable
			foreach (Seat seat in seats)
			{
				await _seatRepository.UpdateAvailabilityAsync(seat.SeatId, false);
			}

			_unitOfWork.Commit();

			await _auditService.LogAsync(userId, "HoldSeats", "Booking", trainId,
				$"Held {seatIds.Count} seats on train {trainId} until {holdExpiresAt:yyyy-MM-dd HH:mm:ss} UTC");

			return (true, $"Seats held successfully. Expires at {holdExpiresAt:yyyy-MM-dd HH:mm:ss} UTC", bookingIds,
				holdExpiresAt);
		}
		catch (Exception ex)
		{
			_unitOfWork.Rollback();
			return (false, $"Failed to hold seats: {ex.Message}", new List<int>(), DateTime.MinValue);
		}
	}

	/// <summary>
	/// Confirms held seats by updating booking status to Confirmed and payment status to Paid.
	/// Validates that bookings belong to the user, are in Pending status, and have not expired.
	/// Clears HoldExpiresAt timestamp to make booking permanent.
	/// Returns detailed booking information including seat numbers and train details.
	/// </summary>
	public async Task<(bool Success, string Message, ConfirmBookingResponse? Data)> ConfirmHeldSeatsAsync(
		int userId, List<int> bookingIds)
	{
		if (bookingIds.Count == 0)
		{
			return (false, "No bookings selected.", null);
		}

		try
		{
			_unitOfWork.BeginTransaction();

			// Validate all bookings exist and belong to user
			foreach (int bookingId in bookingIds)
			{
				Booking? booking = await _bookingRepository.GetByIdAsync(bookingId);
				if (booking == null)
				{
					_unitOfWork.Rollback();
					return (false, $"Booking {bookingId} not found.", null);
				}

				if (booking.UserId != userId)
				{
					_unitOfWork.Rollback();
					return (false, $"Booking {bookingId} does not belong to you.", null);
				}

				if (booking.BookingStatus != "Pending")
				{
					_unitOfWork.Rollback();
					return (false, $"Booking {bookingId} is not in Pending status.", null);
				}

				// Check if hold has expired (with grace period)
				if (booking.HoldExpiresAt.HasValue)
				{
					int gracePeriodSeconds = _configuration.GetValue("Booking:HoldGracePeriodSeconds", 10);
					DateTime graceDeadline = booking.HoldExpiresAt.Value.AddSeconds(gracePeriodSeconds);
					if (DateTime.UtcNow > graceDeadline)
					{
						_unitOfWork.Rollback();
						return (false, $"Hold has expired for booking {bookingId}.", null);
					}
				}
			}

			// Confirm all bookings
			bool confirmed = await _bookingRepository.ConfirmHeldBookingsAsync(bookingIds, userId);
			if (!confirmed)
			{
				_unitOfWork.Rollback();
				return (false, "Failed to confirm bookings. Some bookings may have expired or been modified.", null);
			}

			// Get detailed booking information after confirmation
			List<BookingDetail> bookingDetails = await _bookingRepository.GetBookingDetailsAsync(bookingIds, userId);

			if (bookingDetails.Count == 0)
			{
				_unitOfWork.Rollback();
				return (false, "Failed to retrieve booking details after confirmation.", null);
			}

			_unitOfWork.Commit();

			// Build response with aggregated booking details
			ConfirmBookingResponse response = new ConfirmBookingResponse
			{
				SeatNumbers = bookingDetails.Select(d => d.SeatNumber).OrderBy(s => s).ToList(),
				TotalAmount = bookingDetails.Sum(d => d.TotalAmount),
				TrainNumber = bookingDetails.First().TrainNumber,
				TrainName = bookingDetails.First().TrainName,
				DepartureStation = bookingDetails.First().DepartureStation,
				ArrivalStation = bookingDetails.First().ArrivalStation,
				BookingCount = bookingDetails.Count,
				BookingIds = bookingDetails.Select(d => d.BookingId).ToList()
			};

			await _auditService.LogAsync(userId, "ConfirmHeldSeats", "Booking", null,
				$"Confirmed {bookingIds.Count} held seats: {string.Join(", ", response.SeatNumbers)}");

			return (true, $"Successfully confirmed {bookingIds.Count} bookings.", response);
		}
		catch (Exception ex)
		{
			_unitOfWork.Rollback();
			return (false, $"Failed to confirm held seats: {ex.Message}", null);
		}
	}

	/// <summary>
	/// Releases held seats by updating booking status to Cancelled and releasing seat availability.
	/// Validates that bookings belong to the user and are in Pending status.
	/// Sends SignalR notifications for seat release (handled by BookingHandler).
	/// </summary>
	public async Task<(bool Success, string Message)> ReleaseHeldSeatsAsync(int userId, List<int> bookingIds)
	{
		if (bookingIds.Count == 0)
		{
			return (false, "No bookings selected.");
		}

		try
		{
			_unitOfWork.BeginTransaction();

			// Get all bookings to release seats
			List<Booking> bookings = new List<Booking>();
			foreach (int bookingId in bookingIds)
			{
				Booking? booking = await _bookingRepository.GetByIdAsync(bookingId);
				if (booking == null)
				{
					continue; // Skip non-existent bookings
				}

				if (booking.UserId != userId)
				{
					_unitOfWork.Rollback();
					return (false, $"Booking {bookingId} does not belong to you.");
				}

				if (booking.BookingStatus != "Pending")
				{
					continue; // Skip non-pending bookings
				}

				bookings.Add(booking);
			}

			if (bookings.Count == 0)
			{
				_unitOfWork.Rollback();
				return (false, "No valid pending bookings found to release.");
			}

			// Release bookings
			bool released = await _bookingRepository.ReleaseHeldBookingsAsync(bookingIds, userId);
			if (!released)
			{
				_unitOfWork.Rollback();
				return (false, "Failed to release bookings.");
			}

			// Release seats
			foreach (Booking booking in bookings)
			{
				await _seatRepository.UpdateAvailabilityAsync(booking.SeatId, true);
			}

			_unitOfWork.Commit();

			await _auditService.LogAsync(userId, "ReleaseHeldSeats", "Booking", null,
				$"Released {bookings.Count} held seats");

			return (true, $"Successfully released {bookings.Count} bookings.");
		}
		catch (Exception ex)
		{
			_unitOfWork.Rollback();
			return (false, $"Failed to release held seats: {ex.Message}");
		}
	}

	/// <summary>
	/// Background cleanup service method to release expired holds.
	/// Queries all bookings with HoldExpiresAt in the past and releases them.
	/// Returns count of released bookings and seat information grouped by train for SignalR notifications.
	/// </summary>
	public async Task<(int ReleasedCount, Dictionary<int, List<int>> ReleasedSeatsByTrain)>
		CleanupExpiredHoldsAsync()
	{
		try
		{
			List<Booking> expiredHolds = await _bookingRepository.GetExpiredHoldsAsync();
			if (expiredHolds.Count == 0)
			{
				return (0, new Dictionary<int, List<int>>());
			}

			int releasedCount = 0;
			var releasedSeatsByTrain = new Dictionary<int, List<int>>();

			// Group by user to process in batches
			var groupedByUser = expiredHolds.GroupBy(b => b.UserId);

			foreach (var userGroup in groupedByUser)
			{
				try
				{
					_unitOfWork.BeginTransaction();

					List<int> bookingIds = userGroup.Select(b => b.BookingId).ToList();
					int userId = userGroup.Key;

					// Release bookings
					bool released = await _bookingRepository.ReleaseHeldBookingsAsync(bookingIds, userId);
					if (released)
					{
						// Release seats
						foreach (Booking booking in userGroup)
						{
							await _seatRepository.UpdateAvailabilityAsync(booking.SeatId, true);

							// Track released seats by train for SignalR notifications
							if (!releasedSeatsByTrain.ContainsKey(booking.TrainId))
							{
								releasedSeatsByTrain[booking.TrainId] = new List<int>();
							}

							releasedSeatsByTrain[booking.TrainId].Add(booking.SeatId);
						}

						_unitOfWork.Commit();
						releasedCount += bookingIds.Count;

						await _auditService.LogAsync(userId, "CleanupExpiredHolds", "Booking", null,
							$"Auto-released {bookingIds.Count} expired holds");
					}
					else
					{
						_unitOfWork.Rollback();
					}
				}
				catch (Exception)
				{
					_unitOfWork.Rollback();
					// Continue processing other users
				}
			}

			return (releasedCount, releasedSeatsByTrain);
		}
		catch (Exception)
		{
			return (0, new Dictionary<int, List<int>>());
		}
	}
}