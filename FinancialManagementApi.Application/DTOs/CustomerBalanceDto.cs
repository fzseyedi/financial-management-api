namespace FinancialManagementApi.Application.DTOs;

public sealed record CustomerBalanceDto(
    int CustomerId,
    string CustomerName,
    decimal TotalInvoiced,
    decimal TotalPaid,
    decimal Balance);