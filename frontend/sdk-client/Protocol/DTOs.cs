using System;

namespace sdk_client.Protocol
{
	/// <summary>
	/// Data Transfer Objects for client-server communication.
	/// These DTOs define the structure of data exchanged between clients and the server.
	/// </summary>
	public class RegisterRequest
	{
		public string Username { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string PhoneNumber { get; set; }
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
		public string DepartureStation { get; set; }
		public string ArrivalStation { get; set; }
		public DateTime? DepartureDate { get; set; }
	}

	public class BookTicketRequest
	{
		public string SessionToken { get; set; } = string.Empty;
		public int TrainId { get; set; }
		public int SeatId { get; set; }
	}

	public class CancelBookingRequest
	{
		public string SessionToken { get; set; } = string.Empty;
		public int BookingId { get; set; }
	}

	public class UpdateUserRequest
	{
		public string SessionToken { get; set; } = string.Empty;
		public string FullName { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
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

