namespace FinancialManagementApi.Application.Payments.Commands.RecordPayment;

public sealed record RecordPaymentCommand(
    int CustomerId,
    int InvoiceId,
    DateTime PaymentDate,
    decimal Amount,
    string? ReferenceNumber,
    string? Notes);