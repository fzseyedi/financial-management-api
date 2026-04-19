namespace FinancialManagementApi.Application.Invoices.Commands.UpdateInvoice;

public sealed record UpdateInvoiceCommandItem(
    int Id,
    int ProductId,
    decimal Quantity,
    decimal UnitPrice);
