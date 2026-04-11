using FinancialManagementApi.Application.Invoices.Commands.CreateInvoice;

namespace FinancialManagementApi.Application.Invoices.Commands;

public sealed record CreateInvoiceCommand(
    int CustomerId,
    DateTime InvoiceDate,
    string? Notes,
    IReadOnlyCollection<CreateInvoiceCommandItem> Items);