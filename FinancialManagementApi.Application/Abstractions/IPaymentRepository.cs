using System.Data;
using FinancialManagementApi.Application.DTOs;
using FinancialManagementApi.Domain.Entities;

namespace FinancialManagementApi.Application.Abstractions;

public interface IPaymentRepository
{
    Task<int> CreateAsync(Payment payment, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a payment record within an existing database transaction.
    /// Used for atomic operations ensuring payment is recorded with invoice update.
    /// </summary>
    Task<int> CreateAsync(Payment payment, IDbTransaction transaction, CancellationToken cancellationToken);

    Task<CustomerBalanceDto?> GetCustomerBalanceAsync(int customerId, CancellationToken cancellationToken);
    Task<IEnumerable<UnpaidInvoiceDto>> GetUnpaidInvoicesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Checks if a customer has any payments.
    /// </summary>
    Task<bool> HasCustomerPaymentsAsync(int customerId, CancellationToken cancellationToken);
}