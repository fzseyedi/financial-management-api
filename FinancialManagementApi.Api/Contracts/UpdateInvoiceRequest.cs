namespace FinancialManagementApi.Api.Contracts;

public sealed record UpdateInvoiceRequest(
    int CustomerId,
    DateTime InvoiceDate,
    string? Notes,
    string? Version,
    string? ModifiedBy,
    IReadOnlyCollection<UpdateInvoiceItemRequest> Items);
