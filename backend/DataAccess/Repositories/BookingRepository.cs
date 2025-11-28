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

    public async Task<int> CreateAsync(Booking booking)
    {
        var sql = @"
			INSERT INTO Booking (UserId, TrainId, SeatId, BookingStatus, TotalAmount, PaymentStatus)
			VALUES (@UserId, @TrainId, @SeatId, @BookingStatus, @TotalAmount, @PaymentStatus);
			SELECT CAST(SCOPE_IDENTITY() as int);";
        return await _unitOfWork.Connection.ExecuteScalarAsync<int>(sql, booking, _unitOfWork.Transaction);
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

