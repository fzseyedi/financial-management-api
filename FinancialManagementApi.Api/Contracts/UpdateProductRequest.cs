namespace FinancialManagementApi.Api.Contracts;

public sealed record UpdateProductRequest(
    string Code,
    string Name,
    decimal UnitPrice);