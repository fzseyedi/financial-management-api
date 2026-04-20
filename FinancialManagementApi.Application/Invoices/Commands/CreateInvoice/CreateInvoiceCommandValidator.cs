using FluentValidation;

namespace FinancialManagementApi.Application.Invoices.Commands.CreateInvoice;

public sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0);

        RuleFor(x => x.InvoiceDate)
            .NotEmpty();

        RuleFor(x => x.Notes)
            .MaximumLength(500);

        RuleFor(x => x.Items)
            .NotNull()
            .Must(x => x.Count > 0)
            .WithMessage("Invoice must contain at least one item.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateInvoiceCommandItemValidator());
    }
}

public sealed class CreateInvoiceCommandItemValidator : AbstractValidator<CreateInvoiceCommandItem>
{
    public CreateInvoiceCommandItemValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0);
    }
}