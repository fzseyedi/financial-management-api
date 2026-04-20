namespace FinancialManagementApi.Application.Invoices.Commands.CreateInvoice;

public sealed record CreateInvoiceCommand(
    int CustomerId,
    DateTime InvoiceDate,
    string? Notes,
    IReadOnlyCollection<CreateInvoiceCommandItem> Items);