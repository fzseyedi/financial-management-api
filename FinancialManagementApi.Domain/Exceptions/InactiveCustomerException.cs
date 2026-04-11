namespace FinancialManagementApi.Domain.Exceptions;

public sealed class InactiveCustomerException : DomainException
{
    public InactiveCustomerException(int customerId)
        : base($"Customer with id {customerId} is inactive and cannot be used for this operation.")
    {
    }
}