using backend.Business.Models;
using backend.DataAccess.DbContext;
using backend.DataAccess.UnitOfWork;
using Dapper;

namespace backend.DataAccess.Repositories;

/// <summary>
/// Repository implementation for Train entity operations using Dapper.
/// Handles train schedule management and search queries.
/// </summary>
public class TrainRepository : ITrainRepository
{
	private readonly DapperContext _context;
	private readonly IUnitOfWork _unitOfWork;

	public TrainRepository(DapperContext context, IUnitOfWork unitOfWork)
	{
		_context = context;
		_unitOfWork = unitOfWork;
	}

	public async Task<Train?> GetByIdAsync(int trainId)
	{
		using var connection = _context.CreateConnection();
		var sql = "SELECT * FROM Train WHERE TrainId = @TrainId";
		return await connection.QueryFirstOrDefaultAsync<Train>(sql, new { TrainId = trainId });
	}

	public async Task<Train?> GetByTrainNumberAsync(string trainNumber)
	{
		using var connection = _context.CreateConnection();
		var sql = "SELECT * FROM Train WHERE TrainNumber = @TrainNumber";
		return await connection.QueryFirstOrDefaultAsync<Train>(sql, new { TrainNumber = trainNumber });
	}

	public async Task<IEnumerable<Train>> GetAllAsync()
	{
		using var connection = _context.CreateConnection();
		var sql = "SELECT * FROM Train ORDER BY DepartureTime";
		return await connection.QueryAsync<Train>(sql);
	}

	public async Task<(IEnumerable<Train> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize)
	{
		using var connection = _context.CreateConnection();

		var countSql = "SELECT COUNT(*) FROM Train";
		var totalCount = await connection.ExecuteScalarAsync<int>(countSql);

		var offset = (pageNumber - 1) * pageSize;
		var dataSql = @"
            SELECT * FROM Train
            ORDER BY DepartureTime
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

		var items = await connection.QueryAsync<Train>(dataSql, new { Offset = offset, PageSize = pageSize });

		return (items, totalCount);
	}

	public async Task<IEnumerable<Train>> SearchAsync(string? departureStation, string? arrivalStation,
		DateTime? departureDate, string? status = null)
	{
		using var connection = _context.CreateConnection();
		var sql = "SELECT * FROM Train WHERE 1=1";
		var parameters = new DynamicParameters();

		if (!string.IsNullOrEmpty(status))
		{
			sql += " AND [Status] = @Status";
			parameters.Add("Status", status);
		}
		else
		{
			sql += " AND [Status] = 'Active'";
		}

		if (!string.IsNullOrEmpty(departureStation))
		{
			sql += " AND DepartureStation = @DepartureStation";
			parameters.Add("DepartureStation", departureStation);
		}

		if (!string.IsNullOrEmpty(arrivalStation))
		{
			sql += " AND ArrivalStation = @ArrivalStation";
			parameters.Add("ArrivalStation", arrivalStation);
		}

		if (departureDate.HasValue)
		{
			sql += " AND CAST(DepartureTime AS DATE) = @DepartureDate";
			parameters.Add("DepartureDate", departureDate.Value.Date);
		}

		sql += " ORDER BY DepartureTime";
		return await connection.QueryAsync<Train>(sql, parameters);
	}

	public async Task<(IEnumerable<Train> Items, int TotalCount)> SearchAsync(string? departureStation,
		string? arrivalStation,
		DateTime? departureDate, int pageNumber, int pageSize, string? status = null)
	{
		using var connection = _context.CreateConnection();
		var whereClause = "WHERE 1=1";
		var parameters = new DynamicParameters();

		if (!string.IsNullOrEmpty(status))
		{
			whereClause += " AND [Status] = @Status";
			parameters.Add("Status", status);
		}
		else
		{
			whereClause += " AND [Status] = 'Active'";
		}

		if (!string.IsNullOrEmpty(departureStation))
		{
			whereClause += " AND DepartureStation = @DepartureStation";
			parameters.Add("DepartureStation", departureStation);
		}

		if (!string.IsNullOrEmpty(arrivalStation))
		{
			whereClause += " AND ArrivalStation = @ArrivalStation";
			parameters.Add("ArrivalStation", arrivalStation);
		}

		if (departureDate.HasValue)
		{
			whereClause += " AND CAST(DepartureTime AS DATE) = @DepartureDate";
			parameters.Add("DepartureDate", departureDate.Value.Date);
		}

		var countSql = $"SELECT COUNT(*) FROM Train {whereClause}";
		var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

		var offset = (pageNumber - 1) * pageSize;
		parameters.Add("Offset", offset);
		parameters.Add("PageSize", pageSize);

		var dataSql = $@"
            SELECT * FROM Train
            {whereClause}
            ORDER BY DepartureTime
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

		var items = await connection.QueryAsync<Train>(dataSql, parameters);

		return (items, totalCount);
	}

	public async Task<int> CreateAsync(Train train)
	{
		using var connection = _context.CreateConnection();
		var sql = @"
			INSERT INTO Train (TrainNumber, TrainName, DepartureStation, ArrivalStation, DepartureTime, ArrivalTime, TotalSeats, TicketPrice, [Status])
			VALUES (@TrainNumber, @TrainName, @DepartureStation, @ArrivalStation, @DepartureTime, @ArrivalTime, @TotalSeats, @TicketPrice, 'Active');
			SELECT CAST(SCOPE_IDENTITY() as int);";
		return await connection.ExecuteScalarAsync<int>(sql, train);
	}

	public async Task<bool> UpdateAsync(Train train)
	{
		using var connection = _context.CreateConnection();
		var sql = @"
			UPDATE Train
			SET TrainNumber = @TrainNumber, TrainName = @TrainName, DepartureStation = @DepartureStation,
				ArrivalStation = @ArrivalStation, DepartureTime = @DepartureTime, ArrivalTime = @ArrivalTime,
				TotalSeats = @TotalSeats, TicketPrice = @TicketPrice, [Status] = @Status
			WHERE TrainId = @TrainId";
		var rowsAffected = await connection.ExecuteAsync(sql, train);
		return rowsAffected > 0;
	}

	public async Task<bool> DeleteAsync(int trainId)
	{
		using var connection = _context.CreateConnection();
		var sql = "DELETE FROM Train WHERE TrainId = @TrainId";
		var rowsAffected = await connection.ExecuteAsync(sql, new { TrainId = trainId });
		return rowsAffected > 0;
	}

	public async Task<bool> UpdateStatusAsync(int trainId, string status)
	{
		using var connection = _context.CreateConnection();
		var sql = "UPDATE Train SET [Status] = @Status WHERE TrainId = @TrainId";
		var rowsAffected = await connection.ExecuteAsync(sql, new { TrainId = trainId, Status = status });
		return rowsAffected > 0;
	}
}