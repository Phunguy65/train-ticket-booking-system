using backend.Business.Models;
using backend.DataAccess.DbContext;
using backend.DataAccess.UnitOfWork;
using Dapper;

namespace backend.DataAccess.Repositories;

/// <summary>
/// Repository implementation for Seat entity operations using Dapper.
/// Handles seat availability management with pessimistic locking support for booking operations.
/// </summary>
public class SeatRepository : ISeatRepository
{
    private readonly DapperContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public SeatRepository(DapperContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Seat?> GetByIdAsync(int seatId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Seat WHERE SeatId = @SeatId";
        return await connection.QueryFirstOrDefaultAsync<Seat>(sql, new { SeatId = seatId });
    }

    public async Task<IEnumerable<Seat>> GetByTrainIdAsync(int trainId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Seat WHERE TrainId = @TrainId ORDER BY SeatNumber";
        return await connection.QueryAsync<Seat>(sql, new { TrainId = trainId });
    }

    public async Task<IEnumerable<Seat>> GetAvailableSeatsByTrainIdAsync(int trainId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Seat WHERE TrainId = @TrainId AND IsAvailable = 1 ORDER BY SeatNumber";
        return await connection.QueryAsync<Seat>(sql, new { TrainId = trainId });
    }

    public async Task<Seat?> GetByIdWithLockAsync(int seatId)
    {
        var sql = "SELECT * FROM Seat WITH (UPDLOCK, ROWLOCK) WHERE SeatId = @SeatId";
        return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<Seat>(sql, new { SeatId = seatId },
            _unitOfWork.Transaction);
    }

    public async Task<int> CreateAsync(Seat seat)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
			INSERT INTO Seat (TrainId, SeatNumber, IsAvailable)
			VALUES (@TrainId, @SeatNumber, @IsAvailable);
			SELECT CAST(SCOPE_IDENTITY() as int);";
        return await connection.ExecuteScalarAsync<int>(sql, seat);
    }

    public async Task<bool> UpdateAsync(Seat seat)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
			UPDATE Seat
			SET IsAvailable = @IsAvailable, [Version] = [Version] + 1
			WHERE SeatId = @SeatId";
        var rowsAffected = await connection.ExecuteAsync(sql, seat);
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateAvailabilityAsync(int seatId, bool isAvailable)
    {
        var sql = @"
			UPDATE Seat
			SET IsAvailable = @IsAvailable, [Version] = [Version] + 1
			WHERE SeatId = @SeatId";
        var rowsAffected = await _unitOfWork.Connection.ExecuteAsync(sql,
            new { SeatId = seatId, IsAvailable = isAvailable }, _unitOfWork.Transaction);
        return rowsAffected > 0;
    }

    public async Task<int> CreateSeatsForTrainAsync(int trainId, int totalSeats)
    {
        using var connection = _context.CreateConnection();
        var seats = new List<Seat>();
        for (int i = 1; i <= totalSeats; i++)
        {
            seats.Add(new Seat { TrainId = trainId, SeatNumber = $"S{i:D2}", IsAvailable = true });
        }

        var sql = @"
			INSERT INTO Seat (TrainId, SeatNumber, IsAvailable)
			VALUES (@TrainId, @SeatNumber, @IsAvailable)";
        var rowsAffected = await connection.ExecuteAsync(sql, seats);
        return rowsAffected;
    }
}

