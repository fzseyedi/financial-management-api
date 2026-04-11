namespace FinancialManagementApi.Api.Contracts;

public sealed record RecordPaymentRequest(
    int CustomerId,
    int InvoiceId,
    DateTime PaymentDate,
    decimal Amount,
    string? ReferenceNumber,
    string? Notes);