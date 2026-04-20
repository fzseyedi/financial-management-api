using Dapper;
using Dapper.Contrib.Extensions;
using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Domain.Entities;
using FinancialManagementApi.Infrastructure.Sql;
using Microsoft.Extensions.Logging;

namespace FinancialManagementApi.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(ISqlConnectionFactory connectionFactory, ILogger<ProductRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<int> CreateAsync(Product product, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var newId = await connection.InsertAsync(product);
            _logger.LogInformation("Product created successfully. ProductId: {ProductId}, Code: {Code}, Name: {Name}", newId, product.Code, product.Name);
            return (int)newId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product with code {Code}", product.Code);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var result = await connection.UpdateAsync(product);
            _logger.LogInformation("Product updated successfully. ProductId: {ProductId}, Code: {Code}, Name: {Name}", product.Id, product.Code, product.Name);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with code {Code}", product.Code);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                ProductSql.Delete,
                new { Id = id },
                cancellationToken: cancellationToken);

            var result = await connection.ExecuteAsync(command);
            if (result > 0)
            {
                _logger.LogInformation("Product deleted successfully. ProductId: {ProductId}", id);
            }
            else
            {
                _logger.LogWarning("No product found to delete. ProductId: {ProductId}", id);
            }
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product. ProductId: {ProductId}", id);
            throw;
        }
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
        try
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

            _logger.LogInformation("Products retrieved. TotalCount: {TotalCount}, PageNumber: {PageNumber}, PageSize: {PageSize}, IncludeInactive: {IncludeInactive}", totalCount, pageNumber, pageSize, includeInactive);
            return (products, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products. PageNumber: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);
            throw;
        }
    }
}