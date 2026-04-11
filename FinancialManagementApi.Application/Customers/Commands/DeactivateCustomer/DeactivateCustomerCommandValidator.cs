using FluentValidation;

namespace FinancialManagementApi.Application.Customers.Commands.DeactivateCustomer;

public sealed class DeactivateCustomerCommandValidator : AbstractValidator<DeactivateCustomerCommand>
{
    public DeactivateCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}