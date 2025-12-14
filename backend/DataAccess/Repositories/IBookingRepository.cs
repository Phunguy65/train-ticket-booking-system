using backend.Business.Models;

namespace backend.DataAccess.Repositories;

/// <summary>
/// Repository interface for Booking entity operations.
/// Provides methods for ticket booking management and history tracking.
/// </summary>
public interface IBookingRepository
{
	Task<Booking?> GetByIdAsync(int bookingId);
	Task<IEnumerable<Booking>> GetByUserIdAsync(int userId);
	Task<IEnumerable<Booking>> GetByTrainIdAsync(int trainId);
	Task<IEnumerable<Booking>> GetAllAsync();
	Task<(IEnumerable<Booking> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize);
	Task<int> CreateAsync(Booking booking);
	Task<List<int>> CreateBatchAsync(List<Booking> bookings);
	Task<bool> UpdateAsync(Booking booking);
	Task<bool> UpdateStatusAsync(int bookingId, string bookingStatus, string paymentStatus);
	Task<bool> CancelBookingAsync(int bookingId);
	Task<List<int>> CreateBatchWithHoldAsync(List<Booking> bookings, DateTime holdExpiresAt);
	Task<bool> ConfirmHeldBookingsAsync(List<int> bookingIds, int userId);
	Task<bool> ReleaseHeldBookingsAsync(List<int> bookingIds, int userId);
	Task<List<Booking>> GetExpiredHoldsAsync();
	Task<List<Booking>> GetUserActiveHoldsAsync(int userId);

	/// <summary>
	/// Gets detailed booking information including seat numbers and train details.
	/// Joins Booking, Seat, and Train tables to retrieve complete booking information.
	/// </summary>
	/// <param name="bookingIds">List of booking IDs to retrieve</param>
	/// <param name="userId">User ID to verify ownership</param>
	/// <returns>List of booking details with seat and train information</returns>
	Task<List<BookingDetail>> GetBookingDetailsAsync(List<int> bookingIds, int userId);

	/// <summary>
	/// Gets enriched booking history with complete train and seat information.
	/// Joins Booking, Train, and Seat tables to retrieve full booking history.
	/// </summary>
	/// <param name="userId">User ID to retrieve bookings for</param>
	/// <returns>List of booking history DTOs ordered by booking date (newest first)</returns>
	Task<List<BookingHistory>> GetBookingHistoryAsync(int userId);

	/// <summary>
	/// Gets paginated enriched booking history with complete train and seat information.
	/// Joins Booking, Train, and Seat tables to retrieve full booking history with pagination.
	/// </summary>
	/// <param name="userId">User ID to retrieve bookings for</param>
	/// <param name="pageNumber">Page number (1-based)</param>
	/// <param name="pageSize">Number of items per page</param>
	/// <returns>Tuple containing paginated booking history items and total count</returns>
	Task<(List<BookingHistory> Items, int TotalCount)> GetBookingHistoryAsync(int userId, int pageNumber,
		int pageSize);
}