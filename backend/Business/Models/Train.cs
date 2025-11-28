namespace backend.Business.Models;

/// <summary>
/// Represents a train schedule with route, timing, and seat information.
/// Used for managing train operations and ticket availability.
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

