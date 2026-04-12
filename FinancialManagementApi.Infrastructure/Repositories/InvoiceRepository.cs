using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.DTOs;
using FinancialManagementApi.Domain.Entities;
using FinancialManagementApi.Infrastructure.Sql;

namespace FinancialManagementApi.Infrastructure.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public InvoiceRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
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
            return invoiceId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.UpdateAsync(invoice);
    }

    public async Task<bool> UpdateAsync(Invoice invoice, IDbTransaction transaction, CancellationToken cancellationToken)
    {
        if (transaction is null)
            throw new ArgumentNullException(nameof(transaction));

        return await transaction.Connection!.UpdateAsync(invoice, transaction);
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

        return await connection.QuerySingleOrDefaultAsync<Invoice>(command);
    }

    public async Task<InvoiceDto?> GetDetailsByIdAsync(int id, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var headerCommand = new CommandDefinition(
            InvoiceSql.GetDetailsHeader,
            new { Id = id },
            cancellationToken: cancellationToken);

        var header = await connection.QuerySingleOrDefaultAsync<InvoiceHeaderRow>(headerCommand);
        if (header is null)
            return null;

        var itemsCommand = new CommandDefinition(
            InvoiceSql.GetDetailsItems,
            new { InvoiceId = id },
            cancellationToken: cancellationToken);

        var items = (await connection.QueryAsync<InvoiceItemDto>(itemsCommand)).ToList();

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
            items);
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
    }
}