namespace FinancialManagementApi.Api.Contracts;

public sealed record CreateInvoiceRequest(
    int CustomerId,
    DateTime InvoiceDate,
    string? Notes,
    IReadOnlyCollection<CreateInvoiceItemRequest> Items);