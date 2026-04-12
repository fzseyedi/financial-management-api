using System.Data;

namespace FinancialManagementApi.Application.Abstractions;

/// <summary>
/// Unit of Work pattern for managing database transactions across multiple repositories.
/// Ensures atomic operations for complex business transactions.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Gets the active database transaction.
    /// </summary>
    IDbTransaction? Transaction { get; }

    /// <summary>
    /// Gets the active database connection.
    /// </summary>
    IDbConnection Connection { get; }

    /// <summary>
    /// Begins a new transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the active transaction.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the active transaction.
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an action within a transaction scope.
    /// Automatically commits on success or rolls back on failure.
    /// </summary>
    Task ExecuteInTransactionAsync(
        Func<IDbTransaction, Task> action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an action within a transaction scope and returns a result.
    /// Automatically commits on success or rolls back on failure.
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(
        Func<IDbTransaction, Task<T>> action,
        CancellationToken cancellationToken = default);
}
