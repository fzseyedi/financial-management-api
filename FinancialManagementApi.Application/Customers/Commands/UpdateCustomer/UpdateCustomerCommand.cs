namespace FinancialManagementApi.Application.Customers.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    int Id,
    string Code,
    string Name,
    string? Email,
    string? Phone,
    string? Address);