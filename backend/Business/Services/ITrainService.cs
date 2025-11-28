using backend.Business.Models;

namespace backend.Business.Services;

/// <summary>
/// Service interface for train management operations.
/// Handles train CRUD operations, search, and seat initialization.
/// </summary>
public interface ITrainService
{
	Task<Train?> GetTrainByIdAsync(int trainId);
	Task<IEnumerable<Train>> GetAllTrainsAsync();
	Task<IEnumerable<Train>> SearchTrainsAsync(string? departureStation, string? arrivalStation, DateTime? departureDate);
	Task<(bool Success, string Message, int TrainId)> CreateTrainAsync(Train train);
	Task<(bool Success, string Message)> UpdateTrainAsync(Train train);
	Task<(bool Success, string Message)> DeleteTrainAsync(int trainId);
	Task<(bool Success, string Message)> UpdateTrainStatusAsync(int trainId, string status);
}

