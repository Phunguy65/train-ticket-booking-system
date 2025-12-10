using Newtonsoft.Json.Linq;
using sdk_client.Protocol;
using sdk_client.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sdk_client.Services
{
	/// <summary>
	/// Service for booking operations including ticket booking, cancellation, and history.
	/// Manages seat reservations and booking records.
	/// </summary>
	public class BookingService
	{
		private readonly ApiClient _apiClient;

		/// <summary>
		/// Initializes a new instance of BookingService with an API client.
		/// </summary>
		/// <param name="apiClient">API client for server communication</param>
		public BookingService(ApiClient apiClient)
		{
			_apiClient = apiClient;
		}

		/// <summary>
		/// Retrieves the seat map for a specific train showing seat availability.
		/// </summary>
		/// <param name="trainId">Unique train identifier</param>
		/// <returns>Seat map with availability information</returns>
		public async Task<object?> GetSeatMapAsync(int trainId)
		{
			var request = new GetSeatMapRequest { TrainId = trainId };

			var response = await _apiClient.SendRequestAsync("Booking.GetSeatMap", request).ConfigureAwait(false);
			return response.Data;
		}

		/// <summary>
		/// Books a ticket for a specific train and seat.
		/// Requires an active session token for authentication.
		/// </summary>
		/// <param name="trainId">Unique train identifier</param>
		/// <param name="seatId">Unique seat identifier</param>
		/// <returns>Response containing booking ID and confirmation</returns>
		public async Task<Response> BookTicketAsync(int trainId, int seatId)
		{
			var request = new BookTicketRequest { TrainId = trainId, SeatId = seatId };

			return await _apiClient.SendRequestAsync("Booking.BookTicket", request).ConfigureAwait(false);
		}

		/// <summary>
		/// Books multiple tickets for specific seats on a train.
		/// Requires an active session token for authentication.
		/// All seats must be available or the entire booking fails.
		/// </summary>
		/// <param name="trainId">Unique train identifier</param>
		/// <param name="seatIds">List of seat identifiers to book</param>
		/// <returns>Response containing list of booking IDs and confirmation</returns>
		public async Task<Response> BookMultipleTicketsAsync(int trainId, List<int> seatIds)
		{
			var request = new BookTicketRequest { TrainId = trainId, SeatIds = seatIds };

			return await _apiClient.SendRequestAsync("Booking.BookTicket", request).ConfigureAwait(false);
		}

		/// <summary>
		/// Cancels an existing booking.
		/// Requires an active session token for authentication.
		/// </summary>
		/// <param name="bookingId">Unique booking identifier</param>
		/// <returns>Response indicating cancellation success</returns>
		public async Task<Response> CancelBookingAsync(int bookingId)
		{
			var request = new CancelBookingRequest { BookingId = bookingId };

			return await _apiClient.SendRequestAsync("Booking.CancelBooking", request).ConfigureAwait(false);
		}

		/// <summary>
		/// Retrieves the booking history for the current authenticated user.
		/// DateTime values in the response are converted from UTC to local time.
		/// Requires an active session token for authentication.
		/// </summary>
		/// <returns>List of user's bookings with local time</returns>
		public async Task<object?> GetBookingHistoryAsync()
		{
			var response = await _apiClient.SendRequestAsync("Booking.GetBookingHistory").ConfigureAwait(false);
			return ConvertBookingHistoryTimesToLocal(response.Data);
		}

		/// <summary>
		/// Retrieves paginated booking history for the current authenticated user.
		/// DateTime values in the response are converted from UTC to local time.
		/// Requires an active session token for authentication.
		/// </summary>
		/// <param name="pageNumber">Page number (1-based)</param>
		/// <param name="pageSize">Number of items per page (1-100)</param>
		/// <returns>Paginated list of user's bookings with local time</returns>
		public async Task<object?> GetBookingHistoryAsync(int? pageNumber, int? pageSize)
		{
			object? requestData = null;

			if (pageNumber.HasValue && pageSize.HasValue)
			{
				requestData = new { PageNumber = pageNumber.Value, PageSize = pageSize.Value };
			}

			var response = await _apiClient.SendRequestAsync("Booking.GetBookingHistory", requestData)
				.ConfigureAwait(false);
			return ConvertBookingHistoryTimesToLocal(response.Data);
		}

		/// <summary>
		/// Retrieves all bookings in the system with optional pagination.
		/// DateTime values in the response are converted from UTC to local time.
		/// Requires admin privileges.
		/// </summary>
		/// <param name="pageNumber">Page number (1-based)</param>
		/// <param name="pageSize">Number of items per page (1-100)</param>
		/// <returns>List of all bookings or paginated result with local time</returns>
		public async Task<object?> GetAllBookingsAsync(int? pageNumber = null, int? pageSize = null)
		{
			object? requestData = null;

			if (pageNumber.HasValue && pageSize.HasValue)
			{
				requestData = new { PageNumber = pageNumber.Value, PageSize = pageSize.Value };
			}

			var response = await _apiClient.SendRequestAsync("Booking.GetAllBookings", requestData)
				.ConfigureAwait(false);
			return ConvertBookingTimesToLocal(response.Data);
		}

		/// <summary>
		/// Converts booking DateTime fields from UTC to local time.
		/// Handles both single booking objects and collections (arrays, paginated results).
		/// </summary>
		/// <param name="data">Booking data from server response</param>
		/// <returns>Booking data with DateTime fields converted to local time</returns>
		private object? ConvertBookingTimesToLocal(object? data)
		{
			if (data == null) return null;

			var jToken = data as JToken;
			if (jToken == null) return data;

			if (jToken is JArray jArray)
			{
				foreach (var item in jArray)
				{
					ConvertBookingJObjectTimesToLocal(item as JObject);
				}
			}
			else if (jToken is JObject jObject)
			{
				if (jObject["Items"] != null && jObject["Items"] is JArray items)
				{
					foreach (var item in items)
					{
						ConvertBookingJObjectTimesToLocal(item as JObject);
					}
				}
				else
				{
					ConvertBookingJObjectTimesToLocal(jObject);
				}
			}

			return data;
		}

		/// <summary>
		/// Converts DateTime fields in a single booking JObject from UTC to local time.
		/// </summary>
		/// <param name="bookingObject">Booking JObject</param>
		private void ConvertBookingJObjectTimesToLocal(JObject? bookingObject)
		{
			if (bookingObject == null) return;

			if (bookingObject["BookingDate"] != null)
			{
				var bookingDate = (bookingObject["BookingDate"] ??
				                   throw new InvalidOperationException(
					                   $"{nameof(bookingObject)}[\"BookingDate\"] is null"))
					.Value<DateTime>();
				bookingObject["BookingDate"] = bookingDate.ToLocalTimeSafe();
			}

			if (bookingObject["CancelledAt"] != null)
			{
				var cancelledAt = (bookingObject["CancelledAt"] ??
				                   throw new InvalidOperationException(
					                   $"{nameof(bookingObject)}[\"CancelledAt\"] is null"))
					.Value<DateTime?>();
				if (cancelledAt.HasValue)
				{
					bookingObject["CancelledAt"] = cancelledAt.Value.ToLocalTimeSafe();
				}
			}

			if (bookingObject["HoldExpiresAt"] != null)
			{
				var holdExpiresAt = (bookingObject["HoldExpiresAt"] ??
				                     throw new InvalidOperationException(
					                     $"{nameof(bookingObject)}[\"HoldExpiresAt\"] is null"))
					.Value<DateTime?>();
				if (holdExpiresAt.HasValue)
				{
					bookingObject["HoldExpiresAt"] = holdExpiresAt.Value.ToLocalTimeSafe();
				}
			}
		}

		/// <summary>
		/// Converts booking history DateTime fields from UTC to local time.
		/// Handles BookingHistoryDTO objects with DepartureTime, BookingDate, and CancelledAt.
		/// </summary>
		/// <param name="data">Booking history data from server response</param>
		/// <returns>Booking history data with DateTime fields converted to local time</returns>
		private object? ConvertBookingHistoryTimesToLocal(object? data)
		{
			if (data == null) return null;

			var jToken = data as JToken;
			if (jToken == null) return data;

			if (jToken is JArray jArray)
			{
				foreach (var item in jArray)
				{
					ConvertBookingHistoryJObjectTimesToLocal(item as JObject);
				}
			}
			else if (jToken is JObject jObject)
			{
				ConvertBookingHistoryJObjectTimesToLocal(jObject);
			}

			return data;
		}

		/// <summary>
		/// Converts DateTime fields in a single booking history JObject from UTC to local time.
		/// </summary>
		/// <param name="historyObject">Booking history JObject</param>
		private void ConvertBookingHistoryJObjectTimesToLocal(JObject? historyObject)
		{
			if (historyObject == null) return;

			if (historyObject["DepartureTime"] != null)
			{
				var departureTime = historyObject["DepartureTime"]!.Value<DateTime>();
				historyObject["DepartureTime"] = departureTime.ToLocalTimeSafe();
			}

			if (historyObject["BookingDate"] != null)
			{
				var bookingDate = historyObject["BookingDate"]!.Value<DateTime>();
				historyObject["BookingDate"] = bookingDate.ToLocalTimeSafe();
			}

			if (historyObject["CancelledAt"] != null)
			{
				var cancelledAt = historyObject["CancelledAt"]!.Value<DateTime?>();
				if (cancelledAt.HasValue)
				{
					historyObject["CancelledAt"] = cancelledAt.Value.ToLocalTimeSafe();
				}
			}
		}

		/// <summary>
		/// Temporarily holds seats for a user with configurable timeout.
		/// Creates bookings with Pending status and HoldExpiresAt timestamp.
		/// ExpiresAt timestamp is converted from UTC to local time.
		/// </summary>
		/// <param name="trainId">Unique train identifier</param>
		/// <param name="seatIds">List of seat IDs to hold</param>
		/// <returns>Response containing booking IDs and expiration time in local timezone</returns>
		public async Task<Response> HoldSeatsAsync(int trainId, List<int> seatIds)
		{
			var request = new HoldSeatsRequest { TrainId = trainId, SeatIds = seatIds };

			var response = await _apiClient.SendRequestAsync("Booking.HoldSeats", request).ConfigureAwait(false);

			// Convert ExpiresAt from UTC to local time
			if (response is { Success: true, Data: not null })
			{
				var dataObject = JObject.FromObject(response.Data);
				if (dataObject["ExpiresAt"] != null)
				{
					var expiresAt = dataObject["ExpiresAt"]!.Value<DateTime>();
					dataObject["ExpiresAt"] = expiresAt.ToLocalTimeSafe();
					response.Data = dataObject;
				}
			}

			return response;
		}

		/// <summary>
		/// Confirms held seats by updating booking status to Confirmed and payment status to Paid.
		/// Validates that bookings belong to the user, are in Pending status, and have not expired.
		/// </summary>
		/// <param name="bookingIds">List of booking IDs to confirm</param>
		/// <returns>Response indicating success or failure</returns>
		public async Task<Response> ConfirmHeldSeatsAsync(List<int> bookingIds)
		{
			var request = new ConfirmHeldSeatsRequest { BookingIds = bookingIds };

			var response =
				await _apiClient.SendRequestAsync("Booking.ConfirmHeldSeats", request).ConfigureAwait(false);
			return response;
		}

		/// <summary>
		/// Releases held seats by updating booking status to Cancelled and releasing seat availability.
		/// Validates that bookings belong to the user and are in Pending status.
		/// </summary>
		/// <param name="bookingIds">List of booking IDs to release</param>
		/// <returns>Response indicating success or failure</returns>
		public async Task<Response> ReleaseHeldSeatsAsync(List<int> bookingIds)
		{
			var request = new ReleaseHeldSeatsRequest { BookingIds = bookingIds };

			var response =
				await _apiClient.SendRequestAsync("Booking.ReleaseHeldSeats", request).ConfigureAwait(false);
			return response;
		}
	}
}