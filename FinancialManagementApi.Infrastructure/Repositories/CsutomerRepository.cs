using Dapper;
using Dapper.Contrib.Extensions;
using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Domain.Entities;
using FinancialManagementApi.Infrastructure.Sql;
using Microsoft.Extensions.Logging;

namespace FinancialManagementApi.Infrastructure.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(ISqlConnectionFactory connectionFactory, ILogger<CustomerRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<int> CreateAsync(Customer customer, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var newId = await connection.InsertAsync(customer);
            _logger.LogInformation("Customer created successfully. CustomerId: {CustomerId}, Code: {Code}", newId, customer.Code);
            return (int)newId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer with code {Code}", customer.Code);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Customer customer, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var result = await connection.UpdateAsync(customer);
            _logger.LogInformation("Customer updated successfully. CustomerId: {CustomerId}, Code: {Code}", customer.Id, customer.Code);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer with code {Code}", customer.Code);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                CustomerSql.Delete,
                new { Id = id },
                cancellationToken: cancellationToken);

            var result = await connection.ExecuteAsync(command);
            if (result > 0)
            {
                _logger.LogInformation("Customer deleted successfully. CustomerId: {CustomerId}", id);
            }
            else
            {
                _logger.LogWarning("No customer found to delete. CustomerId: {CustomerId}", id);
            }
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer. CustomerId: {CustomerId}", id);
            throw;
        }
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

    public async Task<(IEnumerable<Customer> Customers, int TotalCount)> GetAllPagedAsync(
        bool includeInactive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = includeInactive
            ? CustomerSql.GetAllIncludingInactivePaged
            : CustomerSql.GetAllActivePaged;

        var countSql = includeInactive
            ? CustomerSql.GetTotalCountIncludingInactive
            : CustomerSql.GetTotalCountActive;

        var parameters = new { PageNumber = pageNumber, PageSize = pageSize };

        var command = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        var countCommand = new CommandDefinition(
            countSql,
            cancellationToken: cancellationToken);

        var customers = await connection.QueryAsync<Customer>(command);
        var totalCount = await connection.ExecuteScalarAsync<int>(countCommand);

        return (customers, totalCount);
    }
}