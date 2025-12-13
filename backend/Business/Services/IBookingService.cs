using backend.Business.Models;
using backend.Presentation.Protocol;

namespace backend.Business.Services;

/// <summary>
/// Service interface for booking operations.
/// Handles ticket booking with pessimistic locking, cancellation, and booking history.
/// </summary>
public interface IBookingService
{
	Task<IEnumerable<Seat>> GetSeatMapAsync(int trainId);
	Task<(bool Success, string Message, int BookingId)> BookTicketAsync(int userId, int trainId, int seatId);

	Task<(bool Success, string Message, List<int> BookingIds)> BookMultipleTicketsAsync(int userId, int trainId,
		List<int> seatIds);

	Task<(bool Success, string Message)> CancelBookingAsync(int bookingId, int userId, bool isAdmin);
	Task<IEnumerable<Booking>> GetBookingHistoryAsync(int userId);
	Task<List<BookingHistory>> GetBookingHistoryDetailedAsync(int userId);

	/// <summary>
	/// Gets paginated detailed booking history for a user.
	/// Returns enriched booking information with train and seat details.
	/// </summary>
	/// <param name="userId">User ID to retrieve bookings for</param>
	/// <param name="pageNumber">Page number (1-based)</param>
	/// <param name="pageSize">Number of items per page</param>
	/// <returns>Paginated result containing booking history items and pagination metadata</returns>
	Task<PagedResult<BookingHistory>> GetBookingHistoryDetailedAsync(int userId, int pageNumber, int pageSize);

	Task<IEnumerable<Booking>> GetAllBookingsAsync();
	Task<PagedResult<Booking>> GetAllBookingsAsync(int pageNumber, int pageSize);
	Task<Booking?> GetBookingByIdAsync(int bookingId);

	Task<(bool Success, string Message, List<int> BookingIds, DateTime ExpiresAt)> HoldSeatsAsync(int userId,
		int trainId, List<int> seatIds);

	/// <summary>
	/// Confirms held seats and returns detailed booking information.
	/// </summary>
	/// <param name="userId">User ID who owns the bookings</param>
	/// <param name="bookingIds">List of booking IDs to confirm</param>
	/// <returns>Success status, message, and booking details if successful</returns>
	Task<(bool Success, string Message, ConfirmBookingResponse? Data)> ConfirmHeldSeatsAsync(int userId,
		List<int> bookingIds);

	Task<(bool Success, string Message)> ReleaseHeldSeatsAsync(int userId, List<int> bookingIds);
	Task<(int ReleasedCount, Dictionary<int, List<int>> ReleasedSeatsByTrain)> CleanupExpiredHoldsAsync();
}