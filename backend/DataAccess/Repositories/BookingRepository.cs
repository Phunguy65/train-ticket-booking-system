using backend.Business.Models;
using backend.DataAccess.DbContext;
using backend.DataAccess.UnitOfWork;
using Dapper;

namespace backend.DataAccess.Repositories;

/// <summary>
/// Repository implementation for Booking entity operations using Dapper.
/// Handles ticket booking management and history tracking queries.
/// </summary>
public class BookingRepository : IBookingRepository
{
	private readonly DapperContext _context;
	private readonly IUnitOfWork _unitOfWork;

	public BookingRepository(DapperContext context, IUnitOfWork unitOfWork)
	{
		_context = context;
		_unitOfWork = unitOfWork;
	}

	public async Task<Booking?> GetByIdAsync(int bookingId)
	{
		using var connection = _context.CreateConnection();
		var sql = "SELECT * FROM Booking WHERE BookingId = @BookingId";
		return await connection.QueryFirstOrDefaultAsync<Booking>(sql, new { BookingId = bookingId });
	}

	public async Task<IEnumerable<Booking>> GetByUserIdAsync(int userId)
	{
		using var connection = _context.CreateConnection();
		var sql = "SELECT * FROM Booking WHERE UserId = @UserId ORDER BY BookingDate DESC";
		return await connection.QueryAsync<Booking>(sql, new { UserId = userId });
	}

	public async Task<IEnumerable<Booking>> GetByTrainIdAsync(int trainId)
	{
		using var connection = _context.CreateConnection();
		var sql = "SELECT * FROM Booking WHERE TrainId = @TrainId ORDER BY BookingDate DESC";
		return await connection.QueryAsync<Booking>(sql, new { TrainId = trainId });
	}

	public async Task<IEnumerable<Booking>> GetAllAsync()
	{
		using var connection = _context.CreateConnection();
		var sql = "SELECT * FROM Booking ORDER BY BookingDate DESC";
		return await connection.QueryAsync<Booking>(sql);
	}

	public async Task<(IEnumerable<Booking> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize)
	{
		using var connection = _context.CreateConnection();

		var countSql = "SELECT COUNT(*) FROM Booking";
		var totalCount = await connection.ExecuteScalarAsync<int>(countSql);

		var offset = (pageNumber - 1) * pageSize;
		var dataSql = @"
            SELECT * FROM Booking
            ORDER BY BookingDate DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

		var items = await connection.QueryAsync<Booking>(dataSql, new { Offset = offset, PageSize = pageSize });

		return (items, totalCount);
	}

	public async Task<int> CreateAsync(Booking booking)
	{
		var sql = @"
			INSERT INTO Booking (UserId, TrainId, SeatId, BookingStatus, TotalAmount, PaymentStatus)
			VALUES (@UserId, @TrainId, @SeatId, @BookingStatus, @TotalAmount, @PaymentStatus);
			SELECT CAST(SCOPE_IDENTITY() as int);";
		return await _unitOfWork.Connection.ExecuteScalarAsync<int>(sql, booking, _unitOfWork.Transaction);
	}

	/// <summary>
	/// Creates multiple bookings in a single batch operation using OUTPUT clause.
	/// Returns all generated booking IDs in the same order as the input bookings.
	/// Uses a single database round trip for better performance and eliminates MARS dependency.
	/// </summary>
	public async Task<List<int>> CreateBatchAsync(List<Booking> bookings)
	{
		if (bookings.Count == 0)
			return new List<int>();

		// Build dynamic VALUES clause with parameterized values
		var valuesClauses = new List<string>();
		var parameters = new DynamicParameters();

		for (int i = 0; i < bookings.Count; i++)
		{
			var booking = bookings[i];
			valuesClauses.Add(
				$"(@UserId{i}, @TrainId{i}, @SeatId{i}, @BookingStatus{i}, @TotalAmount{i}, @PaymentStatus{i})");

			parameters.Add($"@UserId{i}", booking.UserId);
			parameters.Add($"@TrainId{i}", booking.TrainId);
			parameters.Add($"@SeatId{i}", booking.SeatId);
			parameters.Add($"@BookingStatus{i}", booking.BookingStatus);
			parameters.Add($"@TotalAmount{i}", booking.TotalAmount);
			parameters.Add($"@PaymentStatus{i}", booking.PaymentStatus);
		}

		var sql = $@"
			INSERT INTO Booking (UserId, TrainId, SeatId, BookingStatus, TotalAmount, PaymentStatus)
			OUTPUT INSERTED.BookingId
			VALUES {string.Join(", ", valuesClauses)}";

		var ids = await _unitOfWork.Connection.QueryAsync<int>(sql, parameters, _unitOfWork.Transaction);
		return ids.ToList();
	}

	public async Task<bool> UpdateAsync(Booking booking)
	{
		using var connection = _context.CreateConnection();
		var sql = @"
			UPDATE Booking
			SET BookingStatus = @BookingStatus, PaymentStatus = @PaymentStatus, CancelledAt = @CancelledAt
			WHERE BookingId = @BookingId";
		var rowsAffected = await connection.ExecuteAsync(sql, booking);
		return rowsAffected > 0;
	}

	public async Task<bool> UpdateStatusAsync(int bookingId, string bookingStatus, string paymentStatus)
	{
		using var connection = _context.CreateConnection();
		var sql = @"
			UPDATE Booking
			SET BookingStatus = @BookingStatus, PaymentStatus = @PaymentStatus
			WHERE BookingId = @BookingId";
		var rowsAffected = await connection.ExecuteAsync(sql,
			new { BookingId = bookingId, BookingStatus = bookingStatus, PaymentStatus = paymentStatus });
		return rowsAffected > 0;
	}

	public async Task<bool> CancelBookingAsync(int bookingId)
	{
		using var connection = _context.CreateConnection();
		var sql = @"
			UPDATE Booking
			SET BookingStatus = 'Cancelled', PaymentStatus = 'Refunded', CancelledAt = GETUTCDATE()
			WHERE BookingId = @BookingId";
		var rowsAffected = await connection.ExecuteAsync(sql, new { BookingId = bookingId });
		return rowsAffected > 0;
	}

	/// <summary>
	/// Creates multiple bookings with temporary hold status and expiration time.
	/// All bookings are created with BookingStatus = 'Pending' and HoldExpiresAt set to specified UTC time.
	/// Uses single database round trip with OUTPUT clause for performance.
	/// </summary>
	public async Task<List<int>> CreateBatchWithHoldAsync(List<Booking> bookings, DateTime holdExpiresAt)
	{
		if (bookings.Count == 0)
			return new List<int>();

		var valuesClauses = new List<string>();
		var parameters = new DynamicParameters();

		for (int i = 0; i < bookings.Count; i++)
		{
			var booking = bookings[i];
			valuesClauses.Add(
				$"(@UserId{i}, @TrainId{i}, @SeatId{i}, @BookingStatus{i}, @TotalAmount{i}, @PaymentStatus{i}, @HoldExpiresAt{i})");

			parameters.Add($"@UserId{i}", booking.UserId);
			parameters.Add($"@TrainId{i}", booking.TrainId);
			parameters.Add($"@SeatId{i}", booking.SeatId);
			parameters.Add($"@BookingStatus{i}", booking.BookingStatus);
			parameters.Add($"@TotalAmount{i}", booking.TotalAmount);
			parameters.Add($"@PaymentStatus{i}", booking.PaymentStatus);
			parameters.Add($"@HoldExpiresAt{i}", holdExpiresAt);
		}

		var sql = $@"
			INSERT INTO Booking (UserId, TrainId, SeatId, BookingStatus, TotalAmount, PaymentStatus, HoldExpiresAt)
			OUTPUT INSERTED.BookingId
			VALUES {string.Join(", ", valuesClauses)}";

		var ids = await _unitOfWork.Connection.QueryAsync<int>(sql, parameters, _unitOfWork.Transaction);
		return ids.ToList();
	}

	/// <summary>
	/// Confirms held bookings by updating status to Confirmed and clearing hold expiration.
	/// Validates that bookings belong to the specified user and are in Pending status.
	/// Uses transaction from UnitOfWork for atomicity.
	/// </summary>
	public async Task<bool> ConfirmHeldBookingsAsync(List<int> bookingIds, int userId)
	{
		var sql = @"
			UPDATE Booking
			SET BookingStatus = 'Confirmed', PaymentStatus = 'Paid', HoldExpiresAt = NULL
			WHERE BookingId IN @BookingIds
			  AND UserId = @UserId
			  AND BookingStatus = 'Pending'";

		var rowsAffected = await _unitOfWork.Connection.ExecuteAsync(sql,
			new { BookingIds = bookingIds, UserId = userId }, _unitOfWork.Transaction);

		return rowsAffected == bookingIds.Count;
	}

	/// <summary>
	/// Releases held bookings by updating status to Cancelled.
	/// Validates that bookings belong to the specified user and are in Pending status.
	/// Uses transaction from UnitOfWork for atomicity with seat availability updates.
	/// </summary>
	public async Task<bool> ReleaseHeldBookingsAsync(List<int> bookingIds, int userId)
	{
		var sql = @"
			UPDATE Booking
			SET BookingStatus = 'Cancelled', PaymentStatus = 'Refunded', CancelledAt = GETUTCDATE(), HoldExpiresAt = NULL
			WHERE BookingId IN @BookingIds
			  AND UserId = @UserId
			  AND BookingStatus = 'Pending'";

		var rowsAffected = await _unitOfWork.Connection.ExecuteAsync(sql,
			new { BookingIds = bookingIds, UserId = userId }, _unitOfWork.Transaction);

		return rowsAffected > 0;
	}

	/// <summary>
	/// Retrieves all bookings with expired holds for cleanup processing.
	/// Returns bookings where BookingStatus = 'Pending' and HoldExpiresAt is in the past (UTC).
	/// Uses indexed query for performance.
	/// </summary>
	public async Task<List<Booking>> GetExpiredHoldsAsync()
	{
		using var connection = _context.CreateConnection();
		var sql = @"
			SELECT * FROM Booking
			WHERE BookingStatus = 'Pending'
			  AND HoldExpiresAt < GETUTCDATE()
			ORDER BY HoldExpiresAt ASC";

		var bookings = await connection.QueryAsync<Booking>(sql);
		return bookings.ToList();
	}

	/// <summary>
	/// Retrieves all active holds for a specific user.
	/// Returns bookings where BookingStatus = 'Pending' and HoldExpiresAt is in the future (UTC).
	/// Used for enforcing max holds per user limit.
	/// </summary>
	public async Task<List<Booking>> GetUserActiveHoldsAsync(int userId)
	{
		using var connection = _context.CreateConnection();
		var sql = @"
			SELECT * FROM Booking
			WHERE UserId = @UserId
			  AND BookingStatus = 'Pending'
			  AND HoldExpiresAt > GETUTCDATE()
			ORDER BY HoldExpiresAt ASC";

		var bookings = await connection.QueryAsync<Booking>(sql, new { UserId = userId });
		return bookings.ToList();
	}

	/// <summary>
	/// Gets detailed booking information with seat and train data.
	/// Performs JOIN across Booking, Seat, and Train tables to retrieve complete information.
	/// Validates that bookings belong to the specified user.
	/// </summary>
	public async Task<List<BookingDetail>> GetBookingDetailsAsync(List<int> bookingIds, int userId)
	{
		var connection = _unitOfWork.Transaction != null
			? _unitOfWork.Connection
			: _context.CreateConnection();
		var sql = @"
			SELECT
				b.BookingId,
				b.SeatId,
				s.SeatNumber,
				b.TotalAmount,
				b.TrainId,
				t.TrainNumber,
				t.TrainName,
				t.DepartureStation,
				t.ArrivalStation
			FROM Booking b
			INNER JOIN Seat s ON b.SeatId = s.SeatId
			INNER JOIN Train t ON b.TrainId = t.TrainId
			WHERE b.BookingId IN @BookingIds
			  AND b.UserId = @UserId
			ORDER BY s.SeatNumber ASC";

		var details = await connection.QueryAsync<BookingDetail>(sql,
			new { BookingIds = bookingIds, UserId = userId }, _unitOfWork.Transaction);
		return details.ToList();
	}
}