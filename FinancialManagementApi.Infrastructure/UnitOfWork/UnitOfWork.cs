using System.Data;
using FinancialManagementApi.Application.Abstractions;

namespace FinancialManagementApi.Infrastructure.UnitOfWork;

/// <summary>
/// Implements Unit of Work pattern for managing database transactions.
/// Provides a scope for atomic operations across multiple repositories.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;

    public IDbTransaction? Transaction => _transaction;
    public IDbConnection Connection => _connection ?? throw new InvalidOperationException("Connection not initialized.");

    public UnitOfWork(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (_transaction is not null)
            throw new InvalidOperationException("A transaction is already active. Complete or rollback the current transaction before starting a new one.");

        _connection = _connectionFactory.CreateConnection();

        if (_connection is not IDbConnection dbConnection)
            throw new InvalidOperationException("Invalid database connection.");

        if (dbConnection.State != ConnectionState.Open)
            dbConnection.Open();

        _transaction = dbConnection.BeginTransaction();
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (_transaction is null)
            throw new InvalidOperationException("No active transaction to commit.");

        try
        {
            _transaction.Commit();
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (_transaction is null)
            throw new InvalidOperationException("No active transaction to rollback.");

        try
        {
            _transaction.Rollback();
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public async Task ExecuteInTransactionAsync(
        Func<IDbTransaction, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        ThrowIfDisposed();

        await BeginTransactionAsync(cancellationToken);

        try
        {
            if (_transaction is null)
                throw new InvalidOperationException("Failed to initialize transaction.");

            await action(_transaction);
            await CommitAsync(cancellationToken);
        }
        catch
        {
            if (_transaction is not null)
                await RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<IDbTransaction, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        ThrowIfDisposed();

        await BeginTransactionAsync(cancellationToken);

        try
        {
            if (_transaction is null)
                throw new InvalidOperationException("Failed to initialize transaction.");

            var result = await action(_transaction);
            await CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            if (_transaction is not null)
                await RollbackAsync(cancellationToken);
            throw;
        }
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_disposed)
            return;

        if (_transaction is not null)
        {
            try
            {
                _transaction.Rollback();
            }
            finally
            {
                _transaction.Dispose();
            }
        }

        _connection?.Dispose();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(UnitOfWork));
    }
}
