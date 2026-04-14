using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.DTOs;
using FinancialManagementApi.Domain.Entities;
using FinancialManagementApi.Infrastructure.Sql;
using Microsoft.Extensions.Logging;

namespace FinancialManagementApi.Infrastructure.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<PaymentRepository> _logger;

    public PaymentRepository(ISqlConnectionFactory connectionFactory, ILogger<PaymentRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<int> CreateAsync(Payment payment, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var newId = await connection.InsertAsync(payment);
            _logger.LogInformation("Payment created successfully. PaymentId: {PaymentId}, InvoiceId: {InvoiceId}, Amount: {Amount}", newId, payment.InvoiceId, payment.Amount);
            return (int)newId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for InvoiceId: {InvoiceId}, Amount: {Amount}", payment.InvoiceId, payment.Amount);
            throw;
        }
    }

    public async Task<int> CreateAsync(Payment payment, IDbTransaction transaction, CancellationToken cancellationToken)
    {
        try
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            var newId = await transaction.Connection!.InsertAsync(payment, transaction);
            _logger.LogInformation("Payment created successfully within transaction. PaymentId: {PaymentId}, InvoiceId: {InvoiceId}, Amount: {Amount}", newId, payment.InvoiceId, payment.Amount);
            return (int)newId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment within transaction for InvoiceId: {InvoiceId}, Amount: {Amount}", payment.InvoiceId, payment.Amount);
            throw;
        }
    }

    public async Task<CustomerBalanceDto?> GetCustomerBalanceAsync(int customerId, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                PaymentSql.GetCustomerBalance,
                new { CustomerId = customerId },
                cancellationToken: cancellationToken);

            var result = await connection.QuerySingleOrDefaultAsync<CustomerBalanceDto>(command);
            if (result is not null)
            {
                _logger.LogInformation("Customer balance retrieved. CustomerId: {CustomerId}, TotalInvoiced: {TotalInvoiced}, TotalPaid: {TotalPaid}", customerId, result.TotalInvoiced, result.TotalPaid);
            }
            else
            {
                _logger.LogWarning("No balance information found for CustomerId: {CustomerId}", customerId);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer balance for CustomerId: {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<IEnumerable<UnpaidInvoiceDto>> GetUnpaidInvoicesAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                PaymentSql.GetUnpaidInvoices,
                cancellationToken: cancellationToken);

            var result = (await connection.QueryAsync<UnpaidInvoiceDto>(command)).ToList();
            _logger.LogInformation("Unpaid invoices retrieved. Count: {UnpaidInvoiceCount}", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unpaid invoices");
            throw;
        }
    }

    public async Task<bool> HasCustomerPaymentsAsync(int customerId, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                PaymentSql.HasCustomerPayments,
                new { CustomerId = customerId },
                cancellationToken: cancellationToken);

            var result = await connection.ExecuteScalarAsync<bool>(command);
            if (result)
            {
                _logger.LogInformation("Customer has payments. CustomerId: {CustomerId}", customerId);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking customer payments. CustomerId: {CustomerId}", customerId);
            throw;
        }
    }
}
