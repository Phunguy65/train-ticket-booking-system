using Newtonsoft.Json.Linq;
using sdk_client.Protocol;
using sdk_client.Utilities;
using System;
using System.Threading.Tasks;

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
		/// DateTime values in the response are converted from UTC to local time.
		/// </summary>
		/// <param name="pageNumber">Page number (1-based)</param>
		/// <param name="pageSize">Number of items per page (1-100)</param>
		/// <returns>List of trains or paginated result with local time</returns>
		public async Task<object?> GetAllTrainsAsync(int? pageNumber = null, int? pageSize = null)
		{
			object? requestData = null;

			if (pageNumber.HasValue && pageSize.HasValue)
			{
				requestData = new { PageNumber = pageNumber.Value, PageSize = pageSize.Value };
			}

			var response = await _apiClient.SendRequestAsync("Train.GetAllTrains", requestData).ConfigureAwait(false);
			return ConvertTrainTimesToLocal(response.Data);
		}

		/// <summary>
		/// Retrieves a specific train by its ID.
		/// DateTime values in the response are converted from UTC to local time.
		/// </summary>
		/// <param name="trainId">Unique train identifier</param>
		/// <returns>Train information with local time</returns>
		public async Task<object?> GetTrainByIdAsync(int trainId)
		{
			var requestData = new { TrainId = trainId };
			var response = await _apiClient.SendRequestAsync("Train.GetTrainById", requestData).ConfigureAwait(false);
			return ConvertTrainTimesToLocal(response.Data);
		}

		/// <summary>
		/// Searches for trains based on departure/arrival stations and date with optional pagination.
		/// DateTime values in the request are converted from local to UTC.
		/// DateTime values in the response are converted from UTC to local time.
		/// </summary>
		/// <param name="departureStation">Departure station name (optional)</param>
		/// <param name="arrivalStation">Arrival station name (optional)</param>
		/// <param name="departureDate">Departure date in local time (optional)</param>
		/// <param name="pageNumber">Page number (1-based)</param>
		/// <param name="pageSize">Number of items per page (1-100)</param>
		/// <returns>List of matching trains or paginated result with local time</returns>
		public async Task<object?> SearchTrainsAsync(string? departureStation = null, string? arrivalStation = null,
			DateTime? departureDate = null, int? pageNumber = null, int? pageSize = null, string? status = null)
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
				jObject["DepartureDate"] = departureDate.Value.ToUtcSafe();
			}

			if (pageNumber.HasValue)
			{
				jObject["PageNumber"] = pageNumber.Value;
			}

			if (pageSize.HasValue)
			{
				jObject["PageSize"] = pageSize.Value;
			}

			if (!string.IsNullOrEmpty(status))
			{
				jObject["Status"] = status;
			}

			var response = await _apiClient.SendRequestAsync("Train.SearchTrains", jObject).ConfigureAwait(false);
			return ConvertTrainTimesToLocal(response.Data);
		}

		/// <summary>
		/// Creates a new train schedule.
		/// DateTime values in the request are converted from local to UTC.
		/// </summary>
		/// <param name="createTrainRequest">Train creation details with local time</param>
		/// <returns>Response containing created train ID</returns>
		public async Task<Response> CreateTrainAsync(CreateTrainRequest createTrainRequest)
		{
			createTrainRequest.DepartureTime = createTrainRequest.DepartureTime.ToUtcSafe();
			createTrainRequest.ArrivalTime = createTrainRequest.ArrivalTime.ToUtcSafe();
			return await _apiClient.SendRequestAsync("Train.CreateTrain", createTrainRequest).ConfigureAwait(false);
		}

		/// <summary>
		/// Updates an existing train schedule.
		/// DateTime values in the request are converted from local to UTC.
		/// </summary>
		/// <param name="updateTrainRequest">Train update details with local time</param>
		/// <returns>Response indicating update success</returns>
		public async Task<Response> UpdateTrainAsync(UpdateTrainRequest updateTrainRequest)
		{
			updateTrainRequest.DepartureTime = updateTrainRequest.DepartureTime.ToUtcSafe();
			updateTrainRequest.ArrivalTime = updateTrainRequest.ArrivalTime.ToUtcSafe();
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
			var requestData = new { TrainId = trainId, Status = status };
			return await _apiClient.SendRequestAsync("Train.UpdateTrainStatus", requestData).ConfigureAwait(false);
		}

		/// <summary>
		/// Converts train DateTime fields from UTC to local time.
		/// Handles both single train objects and collections (arrays, paginated results).
		/// </summary>
		/// <param name="data">Train data from server response</param>
		/// <returns>Train data with DateTime fields converted to local time</returns>
		private object? ConvertTrainTimesToLocal(object? data)
		{
			if (data == null) return null;

			var jToken = data as JToken;
			if (jToken == null) return data;

			if (jToken is JArray jArray)
			{
				foreach (var item in jArray)
				{
					ConvertTrainJObjectTimesToLocal(item as JObject);
				}
			}
			else if (jToken is JObject jObject)
			{
				if (jObject["Items"] != null && jObject["Items"] is JArray items)
				{
					foreach (var item in items)
					{
						ConvertTrainJObjectTimesToLocal(item as JObject);
					}
				}
				else
				{
					ConvertTrainJObjectTimesToLocal(jObject);
				}
			}

			return data;
		}

		/// <summary>
		/// Converts DateTime fields in a single train JObject from UTC to local time.
		/// </summary>
		/// <param name="trainObject">Train JObject</param>
		private void ConvertTrainJObjectTimesToLocal(JObject? trainObject)
		{
			if (trainObject == null) return;

			if (trainObject["DepartureTime"] != null)
			{
				var departureTime = (trainObject["DepartureTime"] ??
				                     throw new InvalidOperationException(
					                     $"{nameof(trainObject)}[\"DepartureTime\"] is null"))
					.Value<DateTime>();
				trainObject["DepartureTime"] = departureTime.ToLocalTimeSafe();
			}

			if (trainObject["ArrivalTime"] != null)
			{
				var arrivalTime =
					(trainObject["ArrivalTime"] ??
					 throw new InvalidOperationException($"{nameof(trainObject)}[\"ArrivalTime\"] is null"))
					.Value<DateTime>();
				trainObject["ArrivalTime"] = arrivalTime.ToLocalTimeSafe();
			}

			if (trainObject["CreatedAt"] != null)
			{
				var createdAt = (trainObject["CreatedAt"] ??
				                 throw new InvalidOperationException($"{nameof(trainObject)}[\"CreatedAt\"] is null"))
					.Value<DateTime>();
				trainObject["CreatedAt"] = createdAt.ToLocalTimeSafe();
			}
		}
	}
}