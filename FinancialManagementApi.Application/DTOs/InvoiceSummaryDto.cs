namespace FinancialManagementApi.Application.DTOs;

public sealed record InvoiceSummaryDto(
    int Id,
    string InvoiceNumber,
    int CustomerId,
    string CustomerName,
    DateTime InvoiceDate,
    int Status,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount,
    string? Notes,
    DateTime? CreatedAt);
