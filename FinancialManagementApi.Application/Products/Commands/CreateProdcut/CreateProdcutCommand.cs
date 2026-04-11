namespace FinancialManagementApi.Application.Products.Commands.CreateProdcut;

public sealed record CreateProductCommand(
    string Code,
    string Name,
    decimal UnitPrice);