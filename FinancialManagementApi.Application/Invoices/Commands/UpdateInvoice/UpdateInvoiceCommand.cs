namespace FinancialManagementApi.Application.Invoices.Commands.UpdateInvoice;

public sealed record UpdateInvoiceCommand(
    int InvoiceId,
    int CustomerId,
    DateTime InvoiceDate,
    string? Notes,
    byte[] Version,
    string? ModifiedBy,
    IReadOnlyCollection<UpdateInvoiceCommandItem> Items);
