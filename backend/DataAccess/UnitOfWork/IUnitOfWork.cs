using System.Data;

namespace backend.DataAccess.UnitOfWork;

/// <summary>
/// Unit of Work interface for managing database transactions.
/// Provides transaction control methods to ensure atomicity of operations across multiple repositories.
/// </summary>
public interface IUnitOfWork : IDisposable
{
	IDbConnection Connection { get; }
	IDbTransaction? Transaction { get; }
	void BeginTransaction();
	void Commit();
	void Rollback();
}

