using FluentValidation;

namespace FinancialManagementApi.Application.Products.Commands.CreateProdcut;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0);
    }
}