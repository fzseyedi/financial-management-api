using FinancialManagementApi.Domain.Entities;

namespace FinancialManagementApi.Application.Abstractions;

public interface ICustomerRepository
{
    Task<int> CreateAsync(Customer customer, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Customer customer, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken);
    Task<bool> ExistsByCodeAsync(string code, int excludeId, CancellationToken cancellationToken);
    Task<IEnumerable<Customer>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken);
}