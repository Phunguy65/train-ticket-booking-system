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
    Task<bool> UpdateAsync(Booking booking);
    Task<bool> UpdateStatusAsync(int bookingId, string bookingStatus, string paymentStatus);
    Task<bool> CancelBookingAsync(int bookingId);
}

