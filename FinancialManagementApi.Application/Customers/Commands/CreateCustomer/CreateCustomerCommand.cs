namespace FinancialManagementApi.Application.Customers.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(
    string Code,
    string Name,
    string? Email,
    string? Phone,
    string? Address);