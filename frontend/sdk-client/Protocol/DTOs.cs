using System;
using System.Collections.Generic;

namespace sdk_client.Protocol
{
	/// <summary>
	/// Data Transfer Objects for client-server communication.
	/// These DTOs define the structure of data exchanged between clients and the server.
	/// </summary>
	/// <summary>
	/// Represents a paginated result set with metadata.
	/// Used for API responses that support pagination.
	/// </summary>
	public class PagedResult<T>
	{
		public IEnumerable<T> Items { get; set; } = new List<T>();
		public int TotalCount { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
		public bool HasPreviousPage => PageNumber > 1;
		public bool HasNextPage => PageNumber < TotalPages;
	}

	/// <summary>
	/// Represents a train schedule with route, timing, and seat information.
	/// All DateTime properties are in local timezone (converted by TrainService).
	/// </summary>
	public class Train
	{
		public int TrainId { get; set; }
		public string TrainNumber { get; set; } = string.Empty;
		public string TrainName { get; set; } = string.Empty;
		public string DepartureStation { get; set; } = string.Empty;
		public string ArrivalStation { get; set; } = string.Empty;
		public DateTime DepartureTime { get; set; }
		public DateTime ArrivalTime { get; set; }
		public int TotalSeats { get; set; }
		public decimal TicketPrice { get; set; }
		public string Status { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
	}

	/// <summary>
	/// Represents a seat on a specific train with availability status.
	/// </summary>
	public class Seat
	{
		public int SeatId { get; set; }
		public int TrainId { get; set; }
		public string SeatNumber { get; set; } = string.Empty;
		public bool IsAvailable { get; set; }
		public int Version { get; set; }
	}

	public class RegisterRequest
	{
		public string Username { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string? PhoneNumber { get; set; }
	}

	public class LoginRequest
	{
		public string Username { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
	}

	public class LoginResponse
	{
		public string SessionToken { get; set; } = string.Empty;
		public int UserId { get; set; }
		public string Username { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
	}

	public class CreateTrainRequest
	{
		public string TrainNumber { get; set; } = string.Empty;
		public string TrainName { get; set; } = string.Empty;
		public string DepartureStation { get; set; } = string.Empty;
		public string ArrivalStation { get; set; } = string.Empty;
		public DateTime DepartureTime { get; set; }
		public DateTime ArrivalTime { get; set; }
		public int TotalSeats { get; set; }
		public decimal TicketPrice { get; set; }
	}

	public class UpdateTrainRequest
	{
		public int TrainId { get; set; }
		public string TrainNumber { get; set; } = string.Empty;
		public string TrainName { get; set; } = string.Empty;
		public string DepartureStation { get; set; } = string.Empty;
		public string ArrivalStation { get; set; } = string.Empty;
		public DateTime DepartureTime { get; set; }
		public DateTime ArrivalTime { get; set; }
		public int TotalSeats { get; set; }
		public decimal TicketPrice { get; set; }
		public string Status { get; set; } = string.Empty;
	}

	public class SearchTrainRequest
	{
		public string DepartureStation { get; set; } = string.Empty;
		public string ArrivalStation { get; set; } = string.Empty;
		public DateTime? DepartureDate { get; set; }
		public string? Status { get; set; }
	}

	public class BookTicketRequest
	{
		public string SessionToken { get; set; } = string.Empty;
		public int TrainId { get; set; }
		public int SeatId { get; set; }
		public List<int>? SeatIds { get; set; }
	}

	public class CancelBookingRequest
	{
		public string SessionToken { get; set; } = string.Empty;
		public int BookingId { get; set; }
	}

	public class HoldSeatsRequest
	{
		public string SessionToken { get; set; } = string.Empty;
		public int TrainId { get; set; }
		public List<int> SeatIds { get; set; } = new List<int>();
	}

	public class ConfirmHeldSeatsRequest
	{
		public string SessionToken { get; set; } = string.Empty;
		public List<int> BookingIds { get; set; } = new List<int>();
	}

	public class ReleaseHeldSeatsRequest
	{
		public string SessionToken { get; set; } = string.Empty;
		public List<int> BookingIds { get; set; } = new List<int>();
	}

	/// <summary>
	/// Response DTO for confirmed booking details.
	/// Contains aggregated information about confirmed seats including seat numbers, pricing, and train details.
	/// </summary>
	public class ConfirmBookingResponse
	{
		/// <summary>
		/// List of confirmed seat numbers (e.g., ["A1", "A2", "B3"]).
		/// </summary>
		public List<string> SeatNumbers { get; set; } = new List<string>();

		/// <summary>
		/// Total amount for all confirmed bookings.
		/// </summary>
		public decimal TotalAmount { get; set; }

		/// <summary>
		/// Train number (e.g., "SE1").
		/// </summary>
		public string TrainNumber { get; set; } = string.Empty;

		/// <summary>
		/// Train name (e.g., "Thống Nhất").
		/// </summary>
		public string TrainName { get; set; } = string.Empty;

		/// <summary>
		/// Number of bookings confirmed.
		/// </summary>
		public int BookingCount { get; set; }

		/// <summary>
		/// List of confirmed booking IDs.
		/// </summary>
		public List<int> BookingIds { get; set; } = new List<int>();

		/// <summary>
		/// Departure station name.
		/// </summary>
		public string DepartureStation { get; set; } = string.Empty;

		/// <summary>
		/// Arrival station name.
		/// </summary>
		public string ArrivalStation { get; set; } = string.Empty;
	}

	public class UpdateUserRequest
	{
		public string SessionToken { get; set; } = string.Empty;
		public string? FullName { get; set; }
		public string? Email { get; set; }
		public string? PhoneNumber { get; set; }
	}

	public class LockUnlockUserRequest
	{
		public int UserId { get; set; }
		public bool IsActive { get; set; }
	}

	public class GetBookingHistoryRequest
	{
		public string SessionToken { get; set; } = string.Empty;
	}

	public class GetSeatMapRequest
	{
		public int TrainId { get; set; }
	}

	public class GetAuditLogsRequest
	{
		public int? UserId { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
	}

	public class AuthenticatedRequest
	{
		public string SessionToken { get; set; } = string.Empty;
	}

	public class PaginationRequest
	{
		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}

	/// <summary>
	/// SignalR event data for seat booking notifications.
	/// Sent when one or more seats are successfully booked.
	/// </summary>
	public class SeatBookedEvent
	{
		public int TrainId { get; set; }
		public List<int> SeatIds { get; set; } = new List<int>();
	}

	/// <summary>
	/// SignalR event data for seat release notifications.
	/// Sent when bookings are cancelled and seats become available again.
	/// </summary>
	public class SeatReleasedEvent
	{
		public int TrainId { get; set; }
		public List<int> SeatIds { get; set; } = new List<int>();
	}

	/// <summary>
	/// SignalR event data for seat hold notifications.
	/// Sent when one or more seats are temporarily held (Pending status).
	/// </summary>
	public class SeatHeldEvent
	{
		public int TrainId { get; set; }
		public List<int> SeatIds { get; set; } = new List<int>();
	}

	/// <summary>
	/// Represents a ticket booking transaction.
	/// All DateTime properties are in local timezone (converted by BookingService).
	/// </summary>
	public class Booking
	{
		public int BookingId { get; set; }
		public int UserId { get; set; }
		public int TrainId { get; set; }
		public int SeatId { get; set; }
		public string BookingStatus { get; set; } = string.Empty;
		public DateTime BookingDate { get; set; }
		public decimal TotalAmount { get; set; }
		public string PaymentStatus { get; set; } = string.Empty;
		public DateTime? CancelledAt { get; set; }
		public DateTime? HoldExpiresAt { get; set; }
	}
}