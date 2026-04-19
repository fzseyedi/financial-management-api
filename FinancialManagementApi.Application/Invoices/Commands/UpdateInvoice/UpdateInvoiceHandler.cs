using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Domain.Entities;
using FluentValidation;

namespace FinancialManagementApi.Application.Invoices.Commands.UpdateInvoice;

public sealed class UpdateInvoiceHandler
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IValidator<UpdateInvoiceCommand> _validator;

    public UpdateInvoiceHandler(
        IInvoiceRepository invoiceRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IValidator<UpdateInvoiceCommand> validator)
    {
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _validator = validator;
    }

    public async Task HandleAsync(UpdateInvoiceCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var invoice = await _invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice is null)
            throw new NotFoundException($"Invoice with id {command.InvoiceId} was not found.");

        // Check optimistic concurrency
        if (!invoice.Version.SequenceEqual(command.Version))
            throw new ConflictException("Invoice has been modified by another user. Please refresh and try again.");

        var customer = await _customerRepository.GetByIdAsync(command.CustomerId, cancellationToken);
        if (customer is null)
            throw new NotFoundException($"Customer with id {command.CustomerId} was not found.");

        if (!customer.IsActive)
            throw new BadRequestException($"Customer with id {command.CustomerId} is inactive.");

        var items = new List<InvoiceItem>();

        foreach (var item in command.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null)
                throw new NotFoundException($"Product with id {item.ProductId} was not found.");

            if (!product.IsActive)
                throw new BadRequestException($"Product with id {item.ProductId} is inactive.");

            items.Add(new InvoiceItem(
                item.ProductId,
                item.Quantity,
                item.UnitPrice));
        }

        // Update invoice with new values
        invoice.Update(
            command.CustomerId,
            command.InvoiceDate,
            command.Notes,
            command.ModifiedBy);

        // Update items and recalculate total
        var totalAmount = items.Sum(x => x.Quantity * x.UnitPrice);
        invoice.UpdateTotalAmount(totalAmount, command.ModifiedBy);

        await _invoiceRepository.UpdateWithItemsAsync(invoice, items, cancellationToken);
    }
}
