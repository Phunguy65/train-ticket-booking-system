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
}