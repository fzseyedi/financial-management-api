namespace FinancialManagementApi.Api.Contracts;

public sealed record UpdateInvoiceItemRequest(
    int Id,
    int ProductId,
    decimal Quantity,
    decimal UnitPrice);
