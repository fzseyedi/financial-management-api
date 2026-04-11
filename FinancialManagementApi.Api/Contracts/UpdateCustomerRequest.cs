namespace FinancialManagementApi.Api.Contracts;

public sealed record UpdateCustomerRequest(
    string Code,
    string Name,
    string? Email,
    string? Phone,
    string? Address);