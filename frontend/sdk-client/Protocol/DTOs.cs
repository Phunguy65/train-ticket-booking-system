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
}