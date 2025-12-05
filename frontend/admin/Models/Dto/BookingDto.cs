using System;

namespace admin.Models.Dto
{
	public class BookingDto
	{
		public int BookingId { get; set; }
		public int UserId { get; set; }
		public int TrainId { get; set; }
		public int SeatId { get; set; }
		public string BookingStatus { get; set; }  // Pending / Confirmed / Cancelled
		public DateTime BookingDate { get; set; }
		public decimal TotalAmount { get; set; }
		public string PaymentStatus { get; set; }  // Paid / Unpaid / Refunded
		public DateTime? CancelledAt { get; set; }
	}
}
