using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.DTOs;

namespace FinancialManagementApi.Application.Products.Queries;

public sealed class GetAllProductsHandler
{
    private readonly IProductRepository _productRepository;

    public GetAllProductsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ProductDto>> HandleAsync(GetAllProductsQuery query, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllAsync(query.IncludeInactive, cancellationToken);
        return products.Select(x => x.ToDto());
    }

    public async Task<PaginatedResponse<ProductDto>> HandlePagedAsync(GetAllProductsPagedQuery query, CancellationToken cancellationToken)
    {
        var (products, totalCount) = await _productRepository.GetAllPagedAsync(
            query.IncludeInactive,
            query.PageNumber,
            query.PageSize,
            cancellationToken);

        var totalPages = (totalCount + query.PageSize - 1) / query.PageSize;

        return new PaginatedResponse<ProductDto>(
            products.Select(x => x.ToDto()),
            totalCount,
            query.PageNumber,
            query.PageSize,
            totalPages);
    }
}