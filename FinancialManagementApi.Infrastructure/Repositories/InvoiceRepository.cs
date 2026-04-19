using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.DTOs;
using FinancialManagementApi.Domain.Entities;
using FinancialManagementApi.Infrastructure.Sql;
using Microsoft.Extensions.Logging;

namespace FinancialManagementApi.Infrastructure.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<InvoiceRepository> _logger;

    public InvoiceRepository(ISqlConnectionFactory connectionFactory, ILogger<InvoiceRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<int> CreateWithItemsAsync(
        Invoice invoice,
        IReadOnlyCollection<InvoiceItem> items,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        if (connection is not IDbConnection dbConnection)
            throw new InvalidOperationException("Invalid database connection.");

        if (dbConnection.State != ConnectionState.Open)
            dbConnection.Open();

        using var transaction = dbConnection.BeginTransaction();

        try
        {
            var invoiceId = (int)await dbConnection.InsertAsync(invoice, transaction);

            foreach (var item in items)
            {
                item.AssignInvoice(invoiceId);

                var command = new CommandDefinition(
                    InvoiceSql.InsertInvoiceItem,
                    new
                    {
                        item.InvoiceId,
                        item.ProductId,
                        item.Quantity,
                        item.UnitPrice,
                        item.LineTotal
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken);

                await dbConnection.ExecuteAsync(command);
            }

            var totalAmount = items.Sum(x => x.LineTotal);
            invoice.SetTotalAmount(totalAmount);

            typeof(FinancialManagementApi.Domain.Common.BaseEntity)
                .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
                .SetValue(invoice, invoiceId);

            await dbConnection.UpdateAsync(invoice, transaction);

            transaction.Commit();
            _logger.LogInformation("Invoice created successfully with {ItemCount} items. InvoiceId: {InvoiceId}, CustomerId: {CustomerId}, Total: {TotalAmount}", items.Count, invoiceId, invoice.CustomerId, totalAmount);
            return invoiceId;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Error creating invoice with items for CustomerId: {CustomerId}, ItemCount: {ItemCount}", invoice.CustomerId, items.Count);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var updateSql = @"
                UPDATE Invoices
                SET CustomerId = @CustomerId,
                    InvoiceDate = @InvoiceDate,
                    Status = @Status,
                    TotalAmount = @TotalAmount,
                    PaidAmount = @PaidAmount,
                    Notes = @Notes,
                    ModifiedAt = @ModifiedAt,
                    ModifiedBy = @ModifiedBy
                WHERE Id = @Id AND [Version] = @Version";

            var command = new CommandDefinition(
                updateSql,
                new
                {
                    invoice.CustomerId,
                    invoice.InvoiceDate,
                    invoice.Status,
                    invoice.TotalAmount,
                    invoice.PaidAmount,
                    invoice.Notes,
                    invoice.ModifiedAt,
                    invoice.ModifiedBy,
                    invoice.Id,
                    invoice.Version
                },
                cancellationToken: cancellationToken);

            var result = await connection.ExecuteAsync(command);
            if (result > 0)
            {
                _logger.LogInformation("Invoice updated successfully. InvoiceId: {InvoiceId}, CustomerId: {CustomerId}", invoice.Id, invoice.CustomerId);
                return true;
            }
            else
            {
                _logger.LogWarning("No invoice found to update. InvoiceId: {InvoiceId}", invoice.Id);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice. InvoiceId: {InvoiceId}", invoice.Id);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Invoice invoice, IDbTransaction transaction, CancellationToken cancellationToken)
    {
        try
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            var updateSql = @"
                UPDATE Invoices
                SET CustomerId = @CustomerId,
                    InvoiceDate = @InvoiceDate,
                    Status = @Status,
                    TotalAmount = @TotalAmount,
                    PaidAmount = @PaidAmount,
                    Notes = @Notes,
                    ModifiedAt = @ModifiedAt,
                    ModifiedBy = @ModifiedBy
                WHERE Id = @Id AND [Version] = @Version";

            var command = new CommandDefinition(
                updateSql,
                new
                {
                    invoice.CustomerId,
                    invoice.InvoiceDate,
                    invoice.Status,
                    invoice.TotalAmount,
                    invoice.PaidAmount,
                    invoice.Notes,
                    invoice.ModifiedAt,
                    invoice.ModifiedBy,
                    invoice.Id,
                    invoice.Version
                },
                transaction: transaction,
                cancellationToken: cancellationToken);

            var result = await transaction.Connection!.ExecuteAsync(command);
            if (result > 0)
            {
                _logger.LogInformation("Invoice updated successfully within transaction. InvoiceId: {InvoiceId}, CustomerId: {CustomerId}", invoice.Id, invoice.CustomerId);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice within transaction. InvoiceId: {InvoiceId}", invoice.Id);
            throw;
        }
    }

    public async Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            InvoiceSql.GetById,
            new { Id = id },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<Invoice>(command);
    }

    public async Task<Invoice?> GetByIdWithLockAsync(int id, IDbTransaction transaction, CancellationToken cancellationToken)
    {
        try
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            var connection = transaction.Connection;
            if (connection is null)
                throw new InvalidOperationException("Transaction connection is null.");

            // Use WITH (UPDLOCK) for pessimistic locking to prevent concurrent modifications
            var lockingSql = @"SELECT * FROM Invoices WITH (UPDLOCK) WHERE Id = @Id";

            var command = new CommandDefinition(
                lockingSql,
                new { Id = id },
                transaction: transaction,
                cancellationToken: cancellationToken);

            var result = await connection.QuerySingleOrDefaultAsync<Invoice>(command);
            if (result is not null)
            {
                _logger.LogInformation("Invoice retrieved with lock. InvoiceId: {InvoiceId}", id);
            }
            else
            {
                _logger.LogWarning("No invoice found with lock. InvoiceId: {InvoiceId}", id);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice with lock. InvoiceId: {InvoiceId}", id);
            throw;
        }
    }

    public async Task<InvoiceDto?> GetDetailsByIdAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var headerCommand = new CommandDefinition(
                InvoiceSql.GetDetailsHeader,
                new { Id = id },
                cancellationToken: cancellationToken);

            var header = await connection.QuerySingleOrDefaultAsync<InvoiceHeaderRow>(headerCommand);
            if (header is null)
            {
                _logger.LogWarning("Invoice details not found. InvoiceId: {InvoiceId}", id);
                return null;
            }

            var itemsCommand = new CommandDefinition(
                InvoiceSql.GetDetailsItems,
                new { InvoiceId = id },
                cancellationToken: cancellationToken);

            var items = (await connection.QueryAsync<InvoiceItemDto>(itemsCommand)).ToList();

            _logger.LogInformation("Invoice details retrieved successfully. InvoiceId: {InvoiceId}, ItemCount: {ItemCount}", id, items.Count);

            return new InvoiceDto(
                header.Id,
                header.InvoiceNumber,
                header.CustomerId,
                header.CustomerName,
                header.InvoiceDate,
                header.Status,
                header.TotalAmount,
                header.PaidAmount,
                header.RemainingAmount,
                header.Notes,
                header.ModifiedAt,
                header.ModifiedBy,
                Convert.ToBase64String(header.Version),
                items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice details. InvoiceId: {InvoiceId}", id);
            throw;
        }
    }

    public async Task<bool> HasItemsAsync(int invoiceId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            InvoiceSql.HasItems,
            new { InvoiceId = invoiceId },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<decimal> GetInvoiceTotalAsync(int invoiceId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            InvoiceSql.GetInvoiceTotal,
            new { InvoiceId = invoiceId },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<decimal>(command);
    }

    public async Task<bool> HasCustomerInvoicesAsync(int customerId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            InvoiceSql.HasCustomerInvoices,
            new { CustomerId = customerId },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<(IEnumerable<InvoiceSummaryDto> Invoices, int TotalCount)> GetAllPagedAsync(
        int? customerId,
        bool includeIssued,
        DateTime? dateFrom,
        DateTime? dateTo,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var invoicesCommand = new CommandDefinition(
                InvoiceSql.GetAllPaged,
                new
                {
                    CustomerId = customerId,
                    IncludeIssued = includeIssued,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                },
                cancellationToken: cancellationToken);

            var invoices = (await connection.QueryAsync<InvoiceSummaryDto>(invoicesCommand)).ToList();

            var countCommand = new CommandDefinition(
                InvoiceSql.GetTotalCount,
                new
                {
                    CustomerId = customerId,
                    IncludeIssued = includeIssued,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                },
                cancellationToken: cancellationToken);

            var totalCount = await connection.ExecuteScalarAsync<int>(countCommand);

            _logger.LogInformation("Invoices retrieved. TotalCount: {TotalCount}, PageNumber: {PageNumber}, PageSize: {PageSize}, CustomerId: {CustomerId}", totalCount, pageNumber, pageSize, customerId);
            return (invoices, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoices. CustomerId: {CustomerId}, PageNumber: {PageNumber}, PageSize: {PageSize}", customerId, pageNumber, pageSize);
            throw;
        }
    }

    public async Task UpdateWithItemsAsync(
        Invoice invoice,
        IReadOnlyCollection<InvoiceItem> items,
        CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        if (connection is not IDbConnection dbConnection)
            throw new InvalidOperationException("Invalid database connection.");

        if (dbConnection.State != ConnectionState.Open)
            dbConnection.Open();

        using var transaction = dbConnection.BeginTransaction();

        try
        {
            // Update invoice with optimistic concurrency check (excluding timestamp Version column)
            var updateSql = @"
                UPDATE Invoices
                SET CustomerId = @CustomerId,
                    InvoiceDate = @InvoiceDate,
                    Status = @Status,
                    TotalAmount = @TotalAmount,
                    PaidAmount = @PaidAmount,
                    Notes = @Notes,
                    ModifiedAt = @ModifiedAt,
                    ModifiedBy = @ModifiedBy
                WHERE Id = @Id AND [Version] = @Version";

            var updateCommand = new CommandDefinition(
                updateSql,
                new
                {
                    invoice.CustomerId,
                    invoice.InvoiceDate,
                    invoice.Status,
                    invoice.TotalAmount,
                    invoice.PaidAmount,
                    invoice.Notes,
                    invoice.ModifiedAt,
                    invoice.ModifiedBy,
                    invoice.Id,
                    invoice.Version
                },
                transaction: transaction,
                cancellationToken: cancellationToken);

            var result = await dbConnection.ExecuteAsync(updateCommand);
            if (result == 0)
                throw new InvalidOperationException($"Failed to update invoice with id {invoice.Id}. The invoice may have been deleted or the version may have changed.");

            // Delete existing items for this invoice
            var deleteItemsSql = "DELETE FROM InvoiceItems WHERE InvoiceId = @InvoiceId";
            var deleteCommand = new CommandDefinition(
                deleteItemsSql,
                new { InvoiceId = invoice.Id },
                transaction: transaction,
                cancellationToken: cancellationToken);

            await dbConnection.ExecuteAsync(deleteCommand);

            // Insert new items
            foreach (var item in items)
            {
                item.AssignInvoice(invoice.Id);

                var command = new CommandDefinition(
                    InvoiceSql.InsertInvoiceItem,
                    new
                    {
                        item.InvoiceId,
                        item.ProductId,
                        item.Quantity,
                        item.UnitPrice,
                        item.LineTotal
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken);

                await dbConnection.ExecuteAsync(command);
            }

            transaction.Commit();
            _logger.LogInformation("Invoice updated successfully with {ItemCount} items. InvoiceId: {InvoiceId}, CustomerId: {CustomerId}", items.Count, invoice.Id, invoice.CustomerId);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Error updating invoice with items. InvoiceId: {InvoiceId}, CustomerId: {CustomerId}", invoice.Id, invoice.CustomerId);
            throw;
        }
    }

    private sealed class InvoiceHeaderRow
    {
        public int Id { get; init; }
        public string InvoiceNumber { get; init; } = default!;
        public int CustomerId { get; init; }
        public string CustomerName { get; init; } = default!;
        public DateTime InvoiceDate { get; init; }
        public int Status { get; init; }
        public decimal TotalAmount { get; init; }
        public decimal PaidAmount { get; init; }
        public decimal RemainingAmount { get; init; }
        public string? Notes { get; init; }
        public DateTime ModifiedAt { get; init; }
        public string? ModifiedBy { get; init; }
        public byte[] Version { get; init; } = [];
    }
}