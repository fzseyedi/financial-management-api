namespace FinancialManagementApi.Application.DTOs;

public sealed record InvoiceItemDto(
    int ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);