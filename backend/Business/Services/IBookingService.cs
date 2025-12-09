using backend.Business.Models;

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
	Task<IEnumerable<Booking>> GetAllBookingsAsync();
	Task<PagedResult<Booking>> GetAllBookingsAsync(int pageNumber, int pageSize);
	Task<Booking?> GetBookingByIdAsync(int bookingId);

	Task<(bool Success, string Message, List<int> BookingIds, DateTime ExpiresAt)> HoldSeatsAsync(int userId,
		int trainId, List<int> seatIds);

	Task<(bool Success, string Message)> ConfirmHeldSeatsAsync(int userId, List<int> bookingIds);
	Task<(bool Success, string Message)> ReleaseHeldSeatsAsync(int userId, List<int> bookingIds);
	Task<int> CleanupExpiredHoldsAsync();
}