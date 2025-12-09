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
			SET BookingStatus = 'Cancelled', PaymentStatus = 'Refunded', CancelledAt = GETDATE()
			WHERE BookingId = @BookingId";
		var rowsAffected = await connection.ExecuteAsync(sql, new { BookingId = bookingId });
		return rowsAffected > 0;
	}
}