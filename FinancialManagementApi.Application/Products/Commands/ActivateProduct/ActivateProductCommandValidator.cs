using FluentValidation;

namespace FinancialManagementApi.Application.Products.Commands.ActivateProduct;

public sealed class ActivateProductCommandValidator : AbstractValidator<ActivateProductCommand>
{
    public ActivateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}