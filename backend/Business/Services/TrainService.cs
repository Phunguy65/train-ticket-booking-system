using backend.Business.Models;
using backend.DataAccess.Repositories;

namespace backend.Business.Services;

/// <summary>
/// Service implementation for train management operations.
/// Handles train CRUD operations with validation and automatic seat creation.
/// </summary>
public class TrainService : ITrainService
{
	private readonly ITrainRepository _trainRepository;
	private readonly ISeatRepository _seatRepository;
	private readonly IAuditService _auditService;

	public TrainService(
		ITrainRepository trainRepository,
		ISeatRepository seatRepository,
		IAuditService auditService)
	{
		_trainRepository = trainRepository;
		_seatRepository = seatRepository;
		_auditService = auditService;
	}

	public async Task<Train?> GetTrainByIdAsync(int trainId)
	{
		return await _trainRepository.GetByIdAsync(trainId);
	}

	public async Task<IEnumerable<Train>> GetAllTrainsAsync()
	{
		return await _trainRepository.GetAllAsync();
	}

	public async Task<PagedResult<Train>> GetAllTrainsAsync(int pageNumber, int pageSize)
	{
		var (items, totalCount) = await _trainRepository.GetAllAsync(pageNumber, pageSize);
		return new PagedResult<Train>
		{
			Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize
		};
	}

	public async Task<IEnumerable<Train>> SearchTrainsAsync(string? departureStation, string? arrivalStation,
		DateTime? departureDate, string? status = null)
	{
		return await _trainRepository.SearchAsync(departureStation, arrivalStation, departureDate, status);
	}

	public async Task<PagedResult<Train>> SearchTrainsAsync(string? departureStation, string? arrivalStation,
		DateTime? departureDate, int pageNumber, int pageSize, string? status = null)
	{
		var (items, totalCount) =
			await _trainRepository.SearchAsync(departureStation, arrivalStation, departureDate, pageNumber, pageSize,
				status);
		return new PagedResult<Train>
		{
			Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize
		};
	}

	public async Task<(bool Success, string Message, int TrainId)> CreateTrainAsync(Train train)
	{
		var existingTrain = await _trainRepository.GetByTrainNumberAsync(train.TrainNumber);
		if (existingTrain != null)
		{
			return (false, "Train number already exists.", 0);
		}

		if (train.DepartureTime >= train.ArrivalTime)
		{
			return (false, "Departure time must be before arrival time.", 0);
		}

		if (train.TotalSeats <= 0 || train.TotalSeats > 100)
		{
			return (false, "Total seats must be between 1 and 100.", 0);
		}

		var trainId = await _trainRepository.CreateAsync(train);
		await _seatRepository.CreateSeatsForTrainAsync(trainId, train.TotalSeats);
		await _auditService.LogAsync(null, "Train Created", "Train", trainId,
			$"Train {train.TrainNumber} created with {train.TotalSeats} seats.");

		return (true, "Train created successfully.", trainId);
	}

	public async Task<(bool Success, string Message)> UpdateTrainAsync(Train train)
	{
		var existingTrain = await _trainRepository.GetByIdAsync(train.TrainId);
		if (existingTrain == null)
		{
			return (false, "Train not found.");
		}

		if (train.DepartureTime >= train.ArrivalTime)
		{
			return (false, "Departure time must be before arrival time.");
		}

		var success = await _trainRepository.UpdateAsync(train);
		if (success)
		{
			await _auditService.LogAsync(null, "Train Updated", "Train", train.TrainId,
				$"Train {train.TrainNumber} updated.");
			return (true, "Train updated successfully.");
		}

		return (false, "Failed to update train.");
	}

	public async Task<(bool Success, string Message)> DeleteTrainAsync(int trainId)
	{
		var train = await _trainRepository.GetByIdAsync(trainId);
		if (train == null)
		{
			return (false, "Train not found.");
		}

		var success = await _trainRepository.DeleteAsync(trainId);
		if (success)
		{
			await _auditService.LogAsync(null, "Train Deleted", "Train", trainId,
				$"Train {train.TrainNumber} deleted.");
			return (true, "Train deleted successfully.");
		}

		return (false, "Failed to delete train.");
	}

	public async Task<(bool Success, string Message)> UpdateTrainStatusAsync(int trainId, string status)
	{
		var train = await _trainRepository.GetByIdAsync(trainId);
		if (train == null)
		{
			return (false, "Train not found.");
		}

		var success = await _trainRepository.UpdateStatusAsync(trainId, status);
		if (success)
		{
			await _auditService.LogAsync(null, "Train Status Updated", "Train", trainId,
				$"Train {train.TrainNumber} status changed to {status}.");
			return (true, "Train status updated successfully.");
		}

		return (false, "Failed to update train status.");
	}
}