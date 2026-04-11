namespace FinancialManagementApi.Api.Contracts;

public sealed record CreateCustomerRequest(
    string Code,
    string Name,
    string? Email,
    string? Phone,
    string? Address);