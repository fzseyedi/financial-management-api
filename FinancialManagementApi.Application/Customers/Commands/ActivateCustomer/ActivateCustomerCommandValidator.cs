using FluentValidation;

namespace FinancialManagementApi.Application.Customers.Commands.ActivateCustomer;

public sealed class ActivateCustomerCommandValidator : AbstractValidator<ActivateCustomerCommand>
{
    public ActivateCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}