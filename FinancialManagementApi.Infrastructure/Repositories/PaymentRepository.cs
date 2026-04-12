using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.DTOs;
using FinancialManagementApi.Domain.Entities;
using FinancialManagementApi.Infrastructure.Sql;

namespace FinancialManagementApi.Infrastructure.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public PaymentRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(Payment payment, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var newId = await connection.InsertAsync(payment);
        return (int)newId;
    }

    public async Task<int> CreateAsync(Payment payment, IDbTransaction transaction, CancellationToken cancellationToken)
    {
        if (transaction is null)
            throw new ArgumentNullException(nameof(transaction));

        var newId = await transaction.Connection!.InsertAsync(payment, transaction);
        return (int)newId;
    }

    public async Task<CustomerBalanceDto?> GetCustomerBalanceAsync(int customerId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            PaymentSql.GetCustomerBalance,
            new { CustomerId = customerId },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<CustomerBalanceDto>(command);
    }

    public async Task<IEnumerable<UnpaidInvoiceDto>> GetUnpaidInvoicesAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            PaymentSql.GetUnpaidInvoices,
            cancellationToken: cancellationToken);

        return await connection.QueryAsync<UnpaidInvoiceDto>(command);
    }

    public async Task<bool> HasCustomerPaymentsAsync(int customerId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            PaymentSql.HasCustomerPayments,
            new { CustomerId = customerId },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }
}
