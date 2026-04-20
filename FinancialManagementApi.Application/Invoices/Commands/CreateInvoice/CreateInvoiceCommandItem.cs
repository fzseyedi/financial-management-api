namespace FinancialManagementApi.Application.Invoices.Commands.CreateInvoice;

public sealed record CreateInvoiceCommandItem(
    int ProductId,
    decimal Quantity,
    decimal UnitPrice);