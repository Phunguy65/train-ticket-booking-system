using backend.Business.Models;

namespace backend.DataAccess.Repositories;

/// <summary>
/// Repository interface for Train entity operations.
/// Provides methods for train schedule management and search.
/// </summary>
public interface ITrainRepository
{
	Task<Train?> GetByIdAsync(int trainId);
	Task<Train?> GetByTrainNumberAsync(string trainNumber);
	Task<IEnumerable<Train>> GetAllAsync();
	Task<(IEnumerable<Train> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize);
	Task<IEnumerable<Train>> SearchAsync(string? departureStation, string? arrivalStation, DateTime? departureDate);

	Task<(IEnumerable<Train> Items, int TotalCount)> SearchAsync(string? departureStation, string? arrivalStation,
		DateTime? departureDate, int pageNumber, int pageSize);

	Task<int> CreateAsync(Train train);
	Task<bool> UpdateAsync(Train train);
	Task<bool> DeleteAsync(int trainId);
	Task<bool> UpdateStatusAsync(int trainId, string status);
}