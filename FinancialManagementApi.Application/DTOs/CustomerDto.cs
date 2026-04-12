namespace FinancialManagementApi.Application.DTOs;

public sealed record CustomerDto(
    int Id,
    string Code,
    string Name,
    string? Email,
    string? Phone,
    string? Address,
    bool IsActive);