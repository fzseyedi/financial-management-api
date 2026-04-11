namespace FinancialManagementApi.Api.Contracts;

public sealed record CreateInvoiceItemRequest(
    int ProductId,
    decimal Quantity,
    decimal UnitPrice);