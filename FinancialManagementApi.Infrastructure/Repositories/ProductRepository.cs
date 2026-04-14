using Dapper;
using Dapper.Contrib.Extensions;
using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Domain.Entities;
using FinancialManagementApi.Infrastructure.Sql;

namespace FinancialManagementApi.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ProductRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(Product product, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var newId = await connection.InsertAsync(product);
        return (int)newId;
    }

    public async Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.UpdateAsync(product);
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            ProductSql.GetById,
            new { Id = id },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<Product>(command);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            ProductSql.ExistsByCode,
            new { Code = code },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<bool> ExistsByCodeAsync(string code, int excludeId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            ProductSql.ExistsByCodeExcludingId,
            new { Code = code, ExcludeId = excludeId },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = includeInactive
            ? ProductSql.GetAllIncludingInactive
            : ProductSql.GetAllActive;

        var command = new CommandDefinition(
            sql,
            cancellationToken: cancellationToken);

        return await connection.QueryAsync<Product>(command);
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetAllPagedAsync(
        bool includeInactive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = includeInactive
            ? ProductSql.GetAllIncludingInactivePaged
            : ProductSql.GetAllActivePaged;

        var countSql = includeInactive
            ? ProductSql.GetTotalCountIncludingInactive
            : ProductSql.GetTotalCountActive;

        var parameters = new { PageNumber = pageNumber, PageSize = pageSize };

        var command = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        var countCommand = new CommandDefinition(
            countSql,
            cancellationToken: cancellationToken);

        var products = await connection.QueryAsync<Product>(command);
        var totalCount = await connection.ExecuteScalarAsync<int>(countCommand);

        return (products, totalCount);
    }
}