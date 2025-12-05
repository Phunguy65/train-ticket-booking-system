using System;

namespace admin.Models.Dto
{
	public class TrainDto
	{
		public int TrainId { get; set; }
		public string TrainNumber { get; set; }
		public string TrainName { get; set; }
		public string DepartureStation { get; set; }
		public string ArrivalStation { get; set; }
		public DateTime DepartureTime { get; set; }
		public DateTime ArrivalTime { get; set; }
		public int TotalSeats { get; set; }
		public decimal TicketPrice { get; set; }
		public string Status { get; set; }     // Active / Cancelled
	}
}
