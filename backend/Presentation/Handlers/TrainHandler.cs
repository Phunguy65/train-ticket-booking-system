using backend.Business.Models;
using backend.Business.Services;
using backend.Presentation.Protocol;
using Newtonsoft.Json.Linq;

namespace backend.Presentation.Handlers;

/// <summary>
/// Handler for train management commands (CRUD operations, search).
/// Processes train-related requests and returns appropriate responses.
/// </summary>
public class TrainHandler
{
	private readonly ITrainService _trainService;

	public TrainHandler(ITrainService trainService)
	{
		_trainService = trainService;
	}

	public async Task<Response> HandleAsync(string action, JObject? data)
	{
		return action switch
		{
			"GetAllTrains" => await HandleGetAllTrainsAsync(),
			"GetTrainById" => await HandleGetTrainByIdAsync(data),
			"SearchTrains" => await HandleSearchTrainsAsync(data),
			"CreateTrain" => await HandleCreateTrainAsync(data),
			"UpdateTrain" => await HandleUpdateTrainAsync(data),
			"DeleteTrain" => await HandleDeleteTrainAsync(data),
			"UpdateTrainStatus" => await HandleUpdateTrainStatusAsync(data),
			_ => new Response { Success = false, ErrorMessage = "Unknown train action." }
		};
	}

	private async Task<Response> HandleGetAllTrainsAsync()
	{
		var trains = await _trainService.GetAllTrainsAsync();
		return new Response { Success = true, Data = trains };
	}

	private async Task<Response> HandleGetTrainByIdAsync(JObject? data)
	{
		if (data == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid request data." };
		}

		var trainId = data["TrainId"]?.Value<int>();
		if (!trainId.HasValue)
		{
			return new Response { Success = false, ErrorMessage = "TrainId is required." };
		}

		var train = await _trainService.GetTrainByIdAsync(trainId.Value);
		return new Response { Success = train != null, Data = train, ErrorMessage = train == null ? "Train not found." : null };
	}

	private async Task<Response> HandleSearchTrainsAsync(JObject? data)
	{
		if (data == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid request data." };
		}

		var request = data.ToObject<SearchTrainRequest>();
		var trains = await _trainService.SearchTrainsAsync(request?.DepartureStation, request?.ArrivalStation, request?.DepartureDate);
		return new Response { Success = true, Data = trains };
	}

	private async Task<Response> HandleCreateTrainAsync(JObject? data)
	{
		if (data == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid request data." };
		}

		var request = data.ToObject<CreateTrainRequest>();
		if (request == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid train data." };
		}

		var train = new Train
		{
			TrainNumber = request.TrainNumber,
			TrainName = request.TrainName,
			DepartureStation = request.DepartureStation,
			ArrivalStation = request.ArrivalStation,
			DepartureTime = request.DepartureTime,
			ArrivalTime = request.ArrivalTime,
			TotalSeats = request.TotalSeats,
			TicketPrice = request.TicketPrice
		};

		var result = await _trainService.CreateTrainAsync(train);
		return new Response
		{
			Success = result.Success,
			ErrorMessage = result.Success ? null : result.Message,
			Data = result.Success ? new { TrainId = result.TrainId, Message = result.Message } : null
		};
	}

	private async Task<Response> HandleUpdateTrainAsync(JObject? data)
	{
		if (data == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid request data." };
		}

		var request = data.ToObject<UpdateTrainRequest>();
		if (request == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid train data." };
		}

		var train = new Train
		{
			TrainId = request.TrainId,
			TrainNumber = request.TrainNumber,
			TrainName = request.TrainName,
			DepartureStation = request.DepartureStation,
			ArrivalStation = request.ArrivalStation,
			DepartureTime = request.DepartureTime,
			ArrivalTime = request.ArrivalTime,
			TotalSeats = request.TotalSeats,
			TicketPrice = request.TicketPrice,
			Status = request.Status
		};

		var result = await _trainService.UpdateTrainAsync(train);
		return new Response
		{
			Success = result.Success,
			ErrorMessage = result.Success ? null : result.Message,
			Data = result.Success ? new { Message = result.Message } : null
		};
	}

	private async Task<Response> HandleDeleteTrainAsync(JObject? data)
	{
		if (data == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid request data." };
		}

		var trainId = data["TrainId"]?.Value<int>();
		if (!trainId.HasValue)
		{
			return new Response { Success = false, ErrorMessage = "TrainId is required." };
		}

		var result = await _trainService.DeleteTrainAsync(trainId.Value);
		return new Response
		{
			Success = result.Success,
			ErrorMessage = result.Success ? null : result.Message,
			Data = result.Success ? new { Message = result.Message } : null
		};
	}

	private async Task<Response> HandleUpdateTrainStatusAsync(JObject? data)
	{
		if (data == null)
		{
			return new Response { Success = false, ErrorMessage = "Invalid request data." };
		}

		var trainId = data["TrainId"]?.Value<int>();
		var status = data["Status"]?.Value<string>();

		if (!trainId.HasValue || string.IsNullOrEmpty(status))
		{
			return new Response { Success = false, ErrorMessage = "TrainId and Status are required." };
		}

		var result = await _trainService.UpdateTrainStatusAsync(trainId.Value, status);
		return new Response
		{
			Success = result.Success,
			ErrorMessage = result.Success ? null : result.Message,
			Data = result.Success ? new { Message = result.Message } : null
		};
	}
}

