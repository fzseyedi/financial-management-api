using FluentValidation;

namespace FinancialManagementApi.Application.Products.Commands.DeactivateProduct;

public sealed class DeactivateProductCommandValidator : AbstractValidator<DeactivateProductCommand>
{
    public DeactivateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}