using System;
namespace admin.Models.Dto
{
	public class SeatDto
	{
		public int SeatId { get; set; }
		public int TrainId { get; set; }
		public int SeatNumber { get; set; }  // 1–10
		public bool IsAvailable { get; set; }
		public int Version { get; set; }     // Optimistic concurrency
	}
}
