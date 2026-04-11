using FinancialManagementApi.Application.DTOs;
using FinancialManagementApi.Domain.Entities;

namespace FinancialManagementApi.Application.Products;

public static class ProductMappings
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto(
            product.Id,
            product.Code,
            product.Name,
            product.UnitPrice,
            product.IsActive);
    }
}