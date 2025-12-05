
using admin.Models.Dto;
using System;
using System.Collections.Generic;

namespace admin.Mock
{
	public static class MockDatabase
	{
		// ================================
		// USERS
		// ================================
		public static List<UserDto> Users = new List<UserDto>
		{
			new UserDto
			{
				UserId = 1,
				Username = "admin",
				PasswordHash = "HASHED_123456",
				FullName = "Quản trị viên",
				Email = "admin@system.com",
				PhoneNumber = "0901234567",
				Role = "Admin",
				CreatedAt = DateTime.Now.AddDays(-10),
				IsActive = true
			},
			new UserDto
			{
				UserId = 2,
				Username = "taman",
				PasswordHash = "HASHED_abcdef",
				FullName = "Tâm An",
				Email = "taman@example.com",
				PhoneNumber = "0912345678",
				Role = "Customer",
				CreatedAt = DateTime.Now.AddDays(-5),
				IsActive = true
			}
		};

		// ================================
		// TRAINS
		// ================================
		public static List<TrainDto> Trains = new List<TrainDto>
		{
			new TrainDto
			{
				TrainId = 1,
				TrainNumber = "SE01",
				TrainName = "Tàu Thống Nhất SE01",
				DepartureStation = "Sài Gòn",
				ArrivalStation = "Hà Nội",
				DepartureTime = DateTime.Today.AddHours(18),
				ArrivalTime = DateTime.Today.AddDays(1).AddHours(14),
				TotalSeats = 10,
				TicketPrice = 850000,
				Status = "Active"
			},
			new TrainDto
			{
				TrainId = 2,
				TrainNumber = "SE22",
				TrainName = "Tàu SE22",
				DepartureStation = "Đà Nẵng",
				ArrivalStation = "Sài Gòn",
				DepartureTime = DateTime.Today.AddHours(20),
				ArrivalTime = DateTime.Today.AddDays(1).AddHours(10),
				TotalSeats = 10,
				TicketPrice = 620000,
				Status = "Active"
			}
		};

		// ================================
		// SEATS
		// ================================
		public static List<SeatDto> Seats = new List<SeatDto>
		{
            // Train 1 → SE01 (10 seats)
            new SeatDto { SeatId = 1, TrainId = 1, SeatNumber = 1, IsAvailable = true, Version = 1 },
			new SeatDto { SeatId = 2, TrainId = 1, SeatNumber = 2, IsAvailable = false, Version = 1 },
			new SeatDto { SeatId = 3, TrainId = 1, SeatNumber = 3, IsAvailable = true, Version = 1 },
			new SeatDto { SeatId = 4, TrainId = 1, SeatNumber = 4, IsAvailable = true, Version = 1 },
			new SeatDto { SeatId = 5, TrainId = 1, SeatNumber = 5, IsAvailable = false, Version = 2 },

            // Train 2 → SE22 (10 seats)
            new SeatDto { SeatId = 11, TrainId = 2, SeatNumber = 1, IsAvailable = true, Version = 1 },
			new SeatDto { SeatId = 12, TrainId = 2, SeatNumber = 2, IsAvailable = true, Version = 1 },
			new SeatDto { SeatId = 13, TrainId = 2, SeatNumber = 3, IsAvailable = false, Version = 2 }
		};

		// ================================
		// BOOKINGS
		// ================================
		public static List<BookingDto> Bookings = new List<BookingDto>
		{
			new BookingDto
			{
				BookingId = 1,
				UserId = 2,
				TrainId = 1,
				SeatId = 2,
				BookingStatus = "Confirmed",
				BookingDate = DateTime.Now.AddDays(-2),
				TotalAmount = 850000,
				PaymentStatus = "Paid"
			},
			new BookingDto
			{
				BookingId = 2,
				UserId = 2,
				TrainId = 1,
				SeatId = 5,
				BookingStatus = "Pending",
				BookingDate = DateTime.Now.AddHours(-5),
				TotalAmount = 850000,
				PaymentStatus = "Unpaid"
			}
		};

		// ================================
		// AUDIT LOG
		// ================================
		public static List<AuditLogDto> Logs = new List<AuditLogDto>
		{
			new AuditLogDto
			{
				LogId = 1,
				UserId = 1,
				Action = "CreateUser",
				EntityType = "User",
				EntityId = 2,
				Details = "Tạo tài khoản khách hàng mới",
				CreatedAt = DateTime.Now.AddDays(-5)
			},
			new AuditLogDto
			{
				LogId = 2,
				UserId = 2,
				Action = "BookTicket",
				EntityType = "Booking",
				EntityId = 1,
				Details = "Đặt vé SE01 - ghế 2",
				CreatedAt = DateTime.Now.AddDays(-2)
			}
		};
	}
}
