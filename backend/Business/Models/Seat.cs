namespace backend.Business.Models;

/// <summary>
/// Represents a seat on a specific train.
/// Tracks seat availability and version for concurrency control.
/// </summary>
public class Seat
{
	public int SeatId { get; set; }
	public int TrainId { get; set; }
	public string SeatNumber { get; set; } = string.Empty;
	public bool IsAvailable { get; set; }
	public int Version { get; set; }
}

