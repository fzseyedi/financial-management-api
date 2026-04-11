namespace FinancialManagementApi.Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    int Id,
    string Code,
    string Name,
    decimal UnitPrice);