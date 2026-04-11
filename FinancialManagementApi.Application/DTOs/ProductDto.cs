namespace FinancialManagementApi.Application.DTOs;

public sealed record ProductDto(
    int Id,
    string Code,
    string Name,
    decimal UnitPrice,
    bool IsActive);