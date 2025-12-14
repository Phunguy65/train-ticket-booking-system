using Newtonsoft.Json.Linq;
using sdk_client.Utilities;
using System;
using System.Threading.Tasks;

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
		/// DateTime values in the request are converted from local to UTC.
		/// DateTime values in the response are converted from UTC to local time.
		/// Requires admin privileges.
		/// </summary>
		/// <param name="userId">Filter by specific user ID (optional)</param>
		/// <param name="startDate">Filter by start date in local time (optional)</param>
		/// <param name="endDate">Filter by end date in local time (optional)</param>
		/// <param name="pageNumber">Page number (1-based)</param>
		/// <param name="pageSize">Number of items per page (1-100)</param>
		/// <returns>List of audit logs or paginated result with local time</returns>
		public async Task<object?> GetAuditLogsAsync(int? userId = null, DateTime? startDate = null,
			DateTime? endDate = null, int? pageNumber = null, int? pageSize = null)
		{
			var jObject = new JObject();

			if (userId.HasValue)
			{
				jObject["UserId"] = userId.Value;
			}

			if (startDate.HasValue)
			{
				jObject["StartDate"] = startDate.Value.ToUtcSafe();
			}

			if (endDate.HasValue)
			{
				jObject["EndDate"] = endDate.Value.ToUtcSafe();
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
			return ConvertAuditLogTimesToLocal(response.Data);
		}

		/// <summary>
		/// Converts audit log DateTime fields from UTC to local time.
		/// Handles both single audit log objects and collections (arrays, paginated results).
		/// </summary>
		/// <param name="data">Audit log data from server response</param>
		/// <returns>Audit log data with DateTime fields converted to local time</returns>
		private object? ConvertAuditLogTimesToLocal(object? data)
		{
			if (data is null) return null;

			if (data is not JToken jToken) return data;

			switch (jToken)
			{
				case JArray jArray:
					{
						foreach (var item in jArray)
						{
							ConvertAuditLogJObjectTimesToLocal(item as JObject);
						}

						break;
					}
				case JObject jObject when jObject["Items"] != null && jObject["Items"] is JArray items:
					{
						foreach (var item in items)
						{
							ConvertAuditLogJObjectTimesToLocal(item as JObject);
						}

						break;
					}
				case JObject jObject:
					ConvertAuditLogJObjectTimesToLocal(jObject);
					break;
			}

			return data;
		}

		/// <summary>
		/// Converts DateTime fields in a single audit log JObject from UTC to local time.
		/// </summary>
		/// <param name="auditLogObject">Audit log JObject</param>
		private void ConvertAuditLogJObjectTimesToLocal(JObject? auditLogObject)
		{
			if (auditLogObject?["CreatedAt"] != null)
			{
				var createdAt =
					(auditLogObject["CreatedAt"] ??
					 throw new InvalidOperationException($"{nameof(auditLogObject)}[\"CreatedAt\"] is null"))
					.Value<DateTime>();
				auditLogObject["CreatedAt"] = createdAt.ToLocalTimeSafe();
			}
		}
	}
}