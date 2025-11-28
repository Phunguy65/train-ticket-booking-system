using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace backend.DataAccess.DbContext;

/// <summary>
/// Database context for creating SQL Server connections using Dapper.
/// Manages connection pooling and provides IDbConnection instances for repository operations.
/// </summary>
public class DapperContext
{
	private readonly string _connectionString;

	public DapperContext(IConfiguration configuration)
	{
		_connectionString = configuration.GetConnectionString("DefaultConnection")
			?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
	}

	public IDbConnection CreateConnection()
	{
		return new SqlConnection(_connectionString);
	}
}

