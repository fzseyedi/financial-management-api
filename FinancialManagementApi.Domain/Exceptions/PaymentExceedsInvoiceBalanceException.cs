namespace FinancialManagementApi.Domain.Exceptions;

public sealed class PaymentExceedsInvoiceBalanceException : DomainException
{
    public PaymentExceedsInvoiceBalanceException()
        : base("Payment amount cannot exceed remaining invoice balance.")
    {
    }
}