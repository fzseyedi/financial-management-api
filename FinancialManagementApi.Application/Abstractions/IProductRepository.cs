using FinancialManagementApi.Domain.Entities;

namespace FinancialManagementApi.Application.Abstractions;

public interface IProductRepository
{
    Task<int> CreateAsync(Product product, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken);
    Task<bool> ExistsByCodeAsync(string code, int excludeId, CancellationToken cancellationToken);
    Task<IEnumerable<Product>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetAllPagedAsync(
        bool includeInactive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}