using backend.Business.Models;
using backend.Business.Services;
using backend.DataAccess.Repositories;
using backend.Infrastructure.Security;

namespace backend.DataAccess.Seeding;

/// <summary>
/// Service for seeding initial data into the database for development and testing.
/// Creates realistic Vietnamese railway data including users, trains, and bookings.
/// </summary>
public class DatabaseSeeder
{
	private readonly IUserRepository _userRepository;
	private readonly ITrainService _trainService;
	private readonly IBookingService _bookingService;
	private readonly ISeatRepository _seatRepository;
	private readonly IAuditService _auditService;
	private readonly PasswordHasher _passwordHasher;
	private readonly ILogger<DatabaseSeeder> _logger;

	public DatabaseSeeder(
		IUserRepository userRepository,
		ITrainService trainService,
		IBookingService bookingService,
		ISeatRepository seatRepository,
		IAuditService auditService,
		PasswordHasher passwordHasher,
		ILogger<DatabaseSeeder> logger)
	{
		_userRepository = userRepository;
		_trainService = trainService;
		_bookingService = bookingService;
		_seatRepository = seatRepository;
		_auditService = auditService;
		_passwordHasher = passwordHasher;
		_logger = logger;
	}

	public async Task SeedAsync()
	{
		_logger.LogInformation("Starting database seeding...");

		try
		{
			var existingUsers = await _userRepository.GetAllAsync();
			if (existingUsers.Any())
			{
				_logger.LogInformation("Database already contains data. Skipping seed.");
				return;
			}

			await SeedUsersAsync();
			await SeedTrainsAsync();
			await SeedBookingsAsync();

			_logger.LogInformation("Database seeding completed successfully.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred during database seeding.");
			throw;
		}
	}

	private async Task SeedUsersAsync()
	{
		_logger.LogInformation("Seeding users...");

		var users = new List<User>
		{
			new User
			{
				Username = "admin",
				PasswordHash = _passwordHasher.HashPassword("Admin@123"),
				FullName = "System Administrator",
				Email = "admin@trainbooking.vn",
				PhoneNumber = "0901234567",
				Role = "Admin",
				IsActive = true
			},
			new User
			{
				Username = "customer1",
				PasswordHash = _passwordHasher.HashPassword("Customer@123"),
				FullName = "Nguyễn Văn An",
				Email = "nguyenvanan@gmail.com",
				PhoneNumber = "0912345678",
				Role = "Customer",
				IsActive = true
			},
			new User
			{
				Username = "customer2",
				PasswordHash = _passwordHasher.HashPassword("Customer@123"),
				FullName = "Trần Thị Bình",
				Email = "tranthibinh@gmail.com",
				PhoneNumber = "0923456789",
				Role = "Customer",
				IsActive = true
			},
			new User
			{
				Username = "customer3",
				PasswordHash = _passwordHasher.HashPassword("Customer@123"),
				FullName = "Lê Hoàng Cường",
				Email = "lehoangcuong@gmail.com",
				PhoneNumber = "0934567890",
				Role = "Customer",
				IsActive = true
			},
			new User
			{
				Username = "customer4",
				PasswordHash = _passwordHasher.HashPassword("Customer@123"),
				FullName = "Phạm Thị Dung",
				Email = "phamthidung@gmail.com",
				PhoneNumber = "0945678901",
				Role = "Customer",
				IsActive = true
			}
		};

		foreach (var user in users)
		{
			var userId = await _userRepository.CreateAsync(user);
			_logger.LogInformation("Created user: {Username} (ID: {UserId})", user.Username, userId);
		}

		_logger.LogInformation("Seeded {Count} users.", users.Count);
	}

	private async Task SeedTrainsAsync()
	{
		_logger.LogInformation("Seeding trains...");

		var baseDate = DateTime.UtcNow.Date.AddDays(1);

		var trains = new List<Train>
		{
			new Train
			{
				TrainNumber = "SE1",
				TrainName = "Thống Nhất Express",
				DepartureStation = "Hà Nội",
				ArrivalStation = "Sài Gòn (TP.HCM)",
				DepartureTime = baseDate.AddHours(6),
				ArrivalTime = baseDate.AddHours(36),
				TotalSeats = 10,
				TicketPrice = 120.00m,
				Status = "Active"
			},
			new Train
			{
				TrainNumber = "SE3",
				TrainName = "Bắc Nam Express",
				DepartureStation = "Hà Nội",
				ArrivalStation = "Đà Nẵng",
				DepartureTime = baseDate.AddHours(8),
				ArrivalTime = baseDate.AddHours(23),
				TotalSeats = 10,
				TicketPrice = 65.00m,
				Status = "Active"
			},
			new Train
			{
				TrainNumber = "SE5",
				TrainName = "Miền Trung Express",
				DepartureStation = "Đà Nẵng",
				ArrivalStation = "Sài Gòn (TP.HCM)",
				DepartureTime = baseDate.AddHours(10),
				ArrivalTime = baseDate.AddHours(28),
				TotalSeats = 10,
				TicketPrice = 85.00m,
				Status = "Active"
			},
			new Train
			{
				TrainNumber = "SE7",
				TrainName = "Hoàng Hoa Express",
				DepartureStation = "Hà Nội",
				ArrivalStation = "Huế",
				DepartureTime = baseDate.AddHours(7),
				ArrivalTime = baseDate.AddHours(19),
				TotalSeats = 10,
				TicketPrice = 55.00m,
				Status = "Active"
			},
			new Train
			{
				TrainNumber = "SE9",
				TrainName = "Duyên Hải Express",
				DepartureStation = "Nha Trang",
				ArrivalStation = "Sài Gòn (TP.HCM)",
				DepartureTime = baseDate.AddHours(9),
				ArrivalTime = baseDate.AddHours(17),
				TotalSeats = 10,
				TicketPrice = 38.00m,
				Status = "Active"
			},
			new Train
			{
				TrainNumber = "SE2",
				TrainName = "Thống Nhất Return",
				DepartureStation = "Sài Gòn (TP.HCM)",
				ArrivalStation = "Hà Nội",
				DepartureTime = baseDate.AddDays(1).AddHours(6),
				ArrivalTime = baseDate.AddDays(2).AddHours(12),
				TotalSeats = 10,
				TicketPrice = 120.00m,
				Status = "Active"
			},
			new Train
			{
				TrainNumber = "SE11",
				TrainName = "Sài Gòn - Nha Trang",
				DepartureStation = "Sài Gòn (TP.HCM)",
				ArrivalStation = "Nha Trang",
				DepartureTime = baseDate.AddHours(5),
				ArrivalTime = baseDate.AddHours(13),
				TotalSeats = 10,
				TicketPrice = 40.00m,
				Status = "Active"
			},
			new Train
			{
				TrainNumber = "SE13",
				TrainName = "Huế - Đà Nẵng Local",
				DepartureStation = "Huế",
				ArrivalStation = "Đà Nẵng",
				DepartureTime = baseDate.AddHours(14),
				ArrivalTime = baseDate.AddHours(17),
				TotalSeats = 10,
				TicketPrice = 18.00m,
				Status = "Active"
			},
			new Train
			{
				TrainNumber = "SE99",
				TrainName = "Cancelled Test Train",
				DepartureStation = "Hà Nội",
				ArrivalStation = "Đà Nẵng",
				DepartureTime = baseDate.AddDays(-2).AddHours(10),
				ArrivalTime = baseDate.AddDays(-2).AddHours(25),
				TotalSeats = 10,
				TicketPrice = 65.00m,
				Status = "Cancelled"
			},
			new Train
			{
				TrainNumber = "SE100",
				TrainName = "Completed Test Train",
				DepartureStation = "Sài Gòn (TP.HCM)",
				ArrivalStation = "Nha Trang",
				DepartureTime = baseDate.AddDays(-3).AddHours(8),
				ArrivalTime = baseDate.AddDays(-3).AddHours(16),
				TotalSeats = 10,
				TicketPrice = 40.00m,
				Status = "Completed"
			}
		};

		foreach (var train in trains)
		{
			var result = await _trainService.CreateTrainAsync(train);
			if (result.Success)
			{
				_logger.LogInformation("Created train: {TrainNumber} - {TrainName} (ID: {TrainId})",
					train.TrainNumber, train.TrainName, result.TrainId);
			}
			else
			{
				_logger.LogWarning("Failed to create train {TrainNumber}: {Message}",
					train.TrainNumber, result.Message);
			}
		}

		_logger.LogInformation("Seeded {Count} trains.", trains.Count);
	}

	private async Task SeedBookingsAsync()
	{
		_logger.LogInformation("Seeding bookings...");

		var allUsers = await _userRepository.GetAllAsync();
		var customerUsers = allUsers.Where(u => u.Role == "Customer").ToList();

		if (!customerUsers.Any())
		{
			_logger.LogWarning("No customer users found. Skipping booking seeding.");
			return;
		}

		var allTrains = await _trainService.GetAllTrainsAsync();
		var activeTrains = allTrains.Where(t => t.Status == "Active").ToList();

		if (!activeTrains.Any())
		{
			_logger.LogWarning("No active trains found. Skipping booking seeding.");
			return;
		}

		var bookingsCreated = 0;

		foreach (var train in activeTrains.Take(5))
		{
			var seats = await _seatRepository.GetByTrainIdAsync(train.TrainId);
			var availableSeats = seats.Where(s => s.IsAvailable).Take(3).ToList();

			for (int i = 0; i < availableSeats.Count && i < customerUsers.Count; i++)
			{
				var customer = customerUsers[i % customerUsers.Count];
				var seat = availableSeats[i];

				var result = await _bookingService.BookTicketAsync(customer.UserId, train.TrainId, seat.SeatId);

				if (result.Success)
				{
					_logger.LogInformation(
						"Created booking: User {Username} booked seat {SeatNumber} on train {TrainNumber} (Booking ID: {BookingId})",
						customer.Username, seat.SeatNumber, train.TrainNumber, result.BookingId);
					bookingsCreated++;
				}
				else
				{
					_logger.LogWarning("Failed to create booking: {Message}", result.Message);
				}
			}
		}

		_logger.LogInformation("Seeded {Count} bookings.", bookingsCreated);
	}
}