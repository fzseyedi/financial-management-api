namespace FinancialManagementApi.Application.DTOs;

public sealed record UnpaidInvoiceDto(
    int InvoiceId,
    string InvoiceNumber,
    int CustomerId,
    string CustomerName,
    DateTime InvoiceDate,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount,
    int Status);