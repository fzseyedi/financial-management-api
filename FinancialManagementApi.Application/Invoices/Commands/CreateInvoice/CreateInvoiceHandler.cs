using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Domain.Entities;
using FluentValidation;

namespace FinancialManagementApi.Application.Invoices.Commands.CreateInvoice;

public sealed class CreateInvoiceHandler
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IValidator<CreateInvoiceCommand> _validator;

    public CreateInvoiceHandler(
        IInvoiceRepository invoiceRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IValidator<CreateInvoiceCommand> validator)
    {
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _validator = validator;
    }

    public async Task<int> HandleAsync(CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

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

        var invoiceNumber = GenerateInvoiceNumber();

        var invoice = new Invoice(
            invoiceNumber,
            command.CustomerId,
            command.InvoiceDate,
            string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim());

        return await _invoiceRepository.CreateWithItemsAsync(invoice, items, cancellationToken);
    }

    private static string GenerateInvoiceNumber()
    {
        var token = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"INV-{token}";
    }
}