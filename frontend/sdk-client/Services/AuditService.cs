using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace sdk_client.Services
{
	/// <summary>
	/// Service for audit log operations.
	/// Provides methods for retrieving system audit logs with filtering options.
	/// </summary>
	public class AuditService
	{
		private readonly ApiClient _apiClient;

		/// <summary>
		/// Initializes a new instance of AuditService with an API client.
		/// </summary>
		/// <param name="apiClient">API client for server communication</param>
		public AuditService(ApiClient apiClient)
		{
			_apiClient = apiClient;
		}

		/// <summary>
		/// Retrieves audit logs with optional filtering by user ID, date range, and pagination.
		/// Requires admin privileges.
		/// </summary>
		/// <param name="userId">Filter by specific user ID (optional)</param>
		/// <param name="startDate">Filter by start date (optional)</param>
		/// <param name="endDate">Filter by end date (optional)</param>
		/// <param name="pageNumber">Page number (1-based)</param>
		/// <param name="pageSize">Number of items per page (1-100)</param>
		/// <returns>List of audit logs or paginated result</returns>
		public async Task<object> GetAuditLogsAsync(int? userId = null, DateTime? startDate = null, DateTime? endDate = null, int? pageNumber = null, int? pageSize = null)
		{
			var jObject = new JObject();

			if (userId.HasValue)
			{
				jObject["UserId"] = userId.Value;
			}

			if (startDate.HasValue)
			{
				jObject["StartDate"] = startDate.Value;
			}

			if (endDate.HasValue)
			{
				jObject["EndDate"] = endDate.Value;
			}

			if (pageNumber.HasValue)
			{
				jObject["PageNumber"] = pageNumber.Value;
			}

			if (pageSize.HasValue)
			{
				jObject["PageSize"] = pageSize.Value;
			}

			var response = await _apiClient.SendRequestAsync("Audit.GetAuditLogs", jObject).ConfigureAwait(false);
			return response.Data;
		}
	}
}

