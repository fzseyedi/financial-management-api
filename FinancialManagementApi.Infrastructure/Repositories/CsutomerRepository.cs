using Dapper;
using Dapper.Contrib.Extensions;
using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Domain.Entities;
using FinancialManagementApi.Infrastructure.Sql;

namespace FinancialManagementApi.Infrastructure.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public CustomerRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(Customer customer, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var newId = await connection.InsertAsync(customer);
        return (int)newId;
    }

    public async Task<bool> UpdateAsync(Customer customer, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.UpdateAsync(customer);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            CustomerSql.Delete,
            new { Id = id },
            cancellationToken: cancellationToken);

        var result = await connection.ExecuteAsync(command);
        return result > 0;
    }

    public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            CustomerSql.GetById,
            new { Id = id },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<Customer>(command);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            CustomerSql.ExistsByCode,
            new { Code = code },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<bool> ExistsByCodeAsync(string code, int excludeId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            CustomerSql.ExistsByCodeExcludingId,
            new { Code = code, ExcludeId = excludeId },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<IEnumerable<Customer>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = includeInactive
            ? CustomerSql.GetAllIncludingInactive
            : CustomerSql.GetAllActive;

        var command = new CommandDefinition(
            sql,
            cancellationToken: cancellationToken);

        return await connection.QueryAsync<Customer>(command);
    }
}