namespace backend.Business.Models;

/// <summary>
/// Represents detailed booking information with seat and train data.
/// Used for displaying complete booking confirmation details.
/// This is a read-only DTO for query results from joined tables.
/// </summary>
public class BookingDetail
{
	/// <summary>
	/// Unique identifier for the booking.
	/// </summary>
	public int BookingId { get; set; }

	/// <summary>
	/// Unique identifier for the seat.
	/// </summary>
	public int SeatId { get; set; }

	/// <summary>
	/// Seat number (e.g., "A1", "B5").
	/// </summary>
	public string SeatNumber { get; set; } = string.Empty;

	/// <summary>
	/// Total amount for this booking.
	/// </summary>
	public decimal TotalAmount { get; set; }

	/// <summary>
	/// Unique identifier for the train.
	/// </summary>
	public int TrainId { get; set; }

	/// <summary>
	/// Train number (e.g., "SE1", "SE2").
	/// </summary>
	public string TrainNumber { get; set; } = string.Empty;

	/// <summary>
	/// Train name (e.g., "Thống Nhất", "Sài Gòn").
	/// </summary>
	public string TrainName { get; set; } = string.Empty;

	/// <summary>
	/// Departure station name.
	/// </summary>
	public string DepartureStation { get; set; } = string.Empty;

	/// <summary>
	/// Arrival station name.
	/// </summary>
	public string ArrivalStation { get; set; } = string.Empty;
}