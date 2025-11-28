using backend.Business.Models;

namespace backend.DataAccess.Repositories;

/// <summary>
/// Repository interface for Seat entity operations.
/// Provides methods for seat availability management with pessimistic locking support.
/// </summary>
public interface ISeatRepository
{
	Task<Seat?> GetByIdAsync(int seatId);
	Task<IEnumerable<Seat>> GetByTrainIdAsync(int trainId);
	Task<IEnumerable<Seat>> GetAvailableSeatsByTrainIdAsync(int trainId);
	Task<Seat?> GetByIdWithLockAsync(int seatId);
	Task<int> CreateAsync(Seat seat);
	Task<bool> UpdateAsync(Seat seat);
	Task<bool> UpdateAvailabilityAsync(int seatId, bool isAvailable);
	Task<int> CreateSeatsForTrainAsync(int trainId, int totalSeats);
}

