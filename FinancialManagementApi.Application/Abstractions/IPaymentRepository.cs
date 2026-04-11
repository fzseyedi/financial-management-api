using FinancialManagementApi.Application.DTOs;
using FinancialManagementApi.Domain.Entities;

namespace FinancialManagementApi.Application.Abstractions;

public interface IPaymentRepository
{
    Task<int> CreateAsync(Payment payment, CancellationToken cancellationToken);
    Task<CustomerBalanceDto?> GetCustomerBalanceAsync(int customerId, CancellationToken cancellationToken);
    Task<IEnumerable<UnpaidInvoiceDto>> GetUnpaidInvoicesAsync(CancellationToken cancellationToken);
}