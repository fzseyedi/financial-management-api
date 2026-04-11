using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FluentValidation;

namespace FinancialManagementApi.Application.Invoices.Commands;

public sealed class IssueInvoiceHandler
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IValidator<IssueInvoiceCommand> _validator;

    public IssueInvoiceHandler(
        IInvoiceRepository invoiceRepository,
        IValidator<IssueInvoiceCommand> validator)
    {
        _invoiceRepository = invoiceRepository;
        _validator = validator;
    }

    public async Task HandleAsync(IssueInvoiceCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var invoice = await _invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice is null)
            throw new NotFoundException($"Invoice with id {command.InvoiceId} was not found.");

        var hasItems = await _invoiceRepository.HasItemsAsync(command.InvoiceId, cancellationToken);

        invoice.Issue(hasItems);

        var updated = await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        if (!updated)
            throw new BadRequestException("Invoice issue operation failed.");
    }
}