using backend.DataAccess.DbContext;
using System.Data;

namespace backend.DataAccess.UnitOfWork;

/// <summary>
/// Unit of Work implementation for managing database transactions.
/// Ensures atomicity of operations across multiple repositories by providing transaction control.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
	private readonly DapperContext _context;
	private IDbConnection? _connection;
	private IDbTransaction? _transaction;
	private bool _disposed;

	public UnitOfWork(DapperContext context)
	{
		_context = context;
	}

	public IDbConnection Connection
	{
		get
		{
			if (_connection == null)
			{
				_connection = _context.CreateConnection();
				_connection.Open();
			}

			return _connection;
		}
	}

	public IDbTransaction? Transaction => _transaction;

	public void BeginTransaction()
	{
		_transaction = Connection.BeginTransaction();
	}

	public void Commit()
	{
		if (_transaction == null)
		{
			throw new InvalidOperationException("Transaction has not been started.");
		}

		try
		{
			_transaction.Commit();
		}
		catch
		{
			_transaction.Rollback();
			throw;
		}
		finally
		{
			_transaction.Dispose();
			_transaction = null;
		}
	}

	public void Rollback()
	{
		if (_transaction == null)
		{
			throw new InvalidOperationException("Transaction has not been started.");
		}

		_transaction.Rollback();
		_transaction.Dispose();
		_transaction = null;
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_transaction?.Dispose();
				_connection?.Dispose();
			}

			_disposed = true;
		}
	}
}