namespace FinancialManagementApi.Domain.Exceptions;

public sealed class InvoiceCannotBeIssuedException : DomainException
{
    public InvoiceCannotBeIssuedException()
        : base("Invoice must contain at least one item before it can be issued.")
    {
    }
}