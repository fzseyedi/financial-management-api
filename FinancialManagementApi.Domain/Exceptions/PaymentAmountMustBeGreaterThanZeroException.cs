namespace FinancialManagementApi.Domain.Exceptions;

public sealed class PaymentAmountMustBeGreaterThanZeroException : DomainException
{
    public PaymentAmountMustBeGreaterThanZeroException()
        : base("Payment amount must be greater than zero.")
    {
    }
}