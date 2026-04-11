namespace FinancialManagementApi.Domain.Exceptions;

public sealed class InvalidInvoiceStateException : DomainException
{
    public InvalidInvoiceStateException(string message) : base(message)
    {
    }
}