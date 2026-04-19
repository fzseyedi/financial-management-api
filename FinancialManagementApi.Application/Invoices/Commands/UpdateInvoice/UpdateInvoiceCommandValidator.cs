using FluentValidation;

namespace FinancialManagementApi.Application.Invoices.Commands.UpdateInvoice;

public sealed class UpdateInvoiceCommandValidator : AbstractValidator<UpdateInvoiceCommand>
{
    public UpdateInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .GreaterThan(0);

        RuleFor(x => x.CustomerId)
            .GreaterThan(0);

        RuleFor(x => x.InvoiceDate)
            .NotEmpty();

        RuleFor(x => x.Notes)
            .MaximumLength(500);

        RuleFor(x => x.Version)
            .NotNull()
            .NotEmpty()
            .WithMessage("Version is required for optimistic concurrency control.");

        RuleFor(x => x.Items)
            .NotNull()
            .Must(x => x.Count > 0)
            .WithMessage("Invoice must contain at least one item.");

        RuleForEach(x => x.Items)
            .SetValidator(new UpdateInvoiceCommandItemValidator());
    }
}

public sealed class UpdateInvoiceCommandItemValidator : AbstractValidator<UpdateInvoiceCommandItem>
{
    public UpdateInvoiceCommandItemValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.ProductId)
            .GreaterThan(0);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0);
    }
}
