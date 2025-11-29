using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using sdk_client.Protocol;

namespace sdk_client.Services
{
	/// <summary>
	/// Service for train management operations including CRUD operations and search.
	/// Provides methods for managing train schedules and information.
	/// </summary>
	public class TrainService
	{
		private readonly ApiClient _apiClient;

		/// <summary>
		/// Initializes a new instance of TrainService with an API client.
		/// </summary>
		/// <param name="apiClient">API client for server communication</param>
		public TrainService(ApiClient apiClient)
		{
			_apiClient = apiClient;
		}

		/// <summary>
		/// Retrieves all trains with optional pagination.
		/// </summary>
		/// <param name="pageNumber">Page number (1-based)</param>
		/// <param name="pageSize">Number of items per page (1-100)</param>
		/// <returns>List of trains or paginated result</returns>
		public async Task<object> GetAllTrainsAsync(int? pageNumber = null, int? pageSize = null)
		{
			object requestData = null;

			if (pageNumber.HasValue && pageSize.HasValue)
			{
				requestData = new
				{
					PageNumber = pageNumber.Value,
					PageSize = pageSize.Value
				};
			}

			var response = await _apiClient.SendRequestAsync("Train.GetAllTrains", requestData).ConfigureAwait(false);
			return response.Data;
		}

		/// <summary>
		/// Retrieves a specific train by its ID.
		/// </summary>
		/// <param name="trainId">Unique train identifier</param>
		/// <returns>Train information</returns>
		public async Task<object> GetTrainByIdAsync(int trainId)
		{
			var requestData = new { TrainId = trainId };
			var response = await _apiClient.SendRequestAsync("Train.GetTrainById", requestData).ConfigureAwait(false);
			return response.Data;
		}

		/// <summary>
		/// Searches for trains based on departure/arrival stations and date with optional pagination.
		/// </summary>
		/// <param name="departureStation">Departure station name (optional)</param>
		/// <param name="arrivalStation">Arrival station name (optional)</param>
		/// <param name="departureDate">Departure date (optional)</param>
		/// <param name="pageNumber">Page number (1-based)</param>
		/// <param name="pageSize">Number of items per page (1-100)</param>
		/// <returns>List of matching trains or paginated result</returns>
		public async Task<object> SearchTrainsAsync(string departureStation = null, string arrivalStation = null, DateTime? departureDate = null, int? pageNumber = null, int? pageSize = null)
		{
			var jObject = new JObject();

			if (!string.IsNullOrEmpty(departureStation))
			{
				jObject["DepartureStation"] = departureStation;
			}

			if (!string.IsNullOrEmpty(arrivalStation))
			{
				jObject["ArrivalStation"] = arrivalStation;
			}

			if (departureDate.HasValue)
			{
				jObject["DepartureDate"] = departureDate.Value;
			}

			if (pageNumber.HasValue)
			{
				jObject["PageNumber"] = pageNumber.Value;
			}

			if (pageSize.HasValue)
			{
				jObject["PageSize"] = pageSize.Value;
			}

			var response = await _apiClient.SendRequestAsync("Train.SearchTrains", jObject).ConfigureAwait(false);
			return response.Data;
		}

		/// <summary>
		/// Creates a new train schedule.
		/// </summary>
		/// <param name="createTrainRequest">Train creation details</param>
		/// <returns>Response containing created train ID</returns>
		public async Task<Response> CreateTrainAsync(CreateTrainRequest createTrainRequest)
		{
			return await _apiClient.SendRequestAsync("Train.CreateTrain", createTrainRequest).ConfigureAwait(false);
		}

		/// <summary>
		/// Updates an existing train schedule.
		/// </summary>
		/// <param name="updateTrainRequest">Train update details</param>
		/// <returns>Response indicating update success</returns>
		public async Task<Response> UpdateTrainAsync(UpdateTrainRequest updateTrainRequest)
		{
			return await _apiClient.SendRequestAsync("Train.UpdateTrain", updateTrainRequest).ConfigureAwait(false);
		}

		/// <summary>
		/// Deletes a train schedule.
		/// </summary>
		/// <param name="trainId">Unique train identifier</param>
		/// <returns>Response indicating deletion success</returns>
		public async Task<Response> DeleteTrainAsync(int trainId)
		{
			var requestData = new { TrainId = trainId };
			return await _apiClient.SendRequestAsync("Train.DeleteTrain", requestData).ConfigureAwait(false);
		}

		/// <summary>
		/// Updates the status of a train (e.g., Active, Cancelled, Delayed).
		/// </summary>
		/// <param name="trainId">Unique train identifier</param>
		/// <param name="status">New train status</param>
		/// <returns>Response indicating status update success</returns>
		public async Task<Response> UpdateTrainStatusAsync(int trainId, string status)
		{
			var requestData = new
			{
				TrainId = trainId,
				Status = status
			};
			return await _apiClient.SendRequestAsync("Train.UpdateTrainStatus", requestData).ConfigureAwait(false);
		}
	}
}

