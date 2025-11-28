namespace backend.Business.Models;

/// <summary>
/// Represents a ticket booking transaction.
/// Links a user to a specific seat on a train with payment and status information.
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
}

