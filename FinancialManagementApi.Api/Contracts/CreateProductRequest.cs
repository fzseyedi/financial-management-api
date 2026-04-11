namespace FinancialManagementApi.Api.Contracts;

public sealed record CreateProductRequest(
    string Code,
    string Name,
    decimal UnitPrice);