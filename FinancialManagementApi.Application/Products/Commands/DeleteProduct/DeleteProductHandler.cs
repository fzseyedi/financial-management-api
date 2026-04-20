using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;

namespace FinancialManagementApi.Application.Products.Commands.DeleteProduct;

public sealed class DeleteProductHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public DeleteProductHandler(
        IProductRepository productRepository,
        IInvoiceRepository invoiceRepository)
    {
        _productRepository = productRepository;
        _invoiceRepository = invoiceRepository;
    }

    public async Task HandleAsync(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
            throw new NotFoundException($"Product with id {command.Id} was not found.");

        var hasInvoiceItems = await _invoiceRepository.HasProductInvoiceItemsAsync(command.Id, cancellationToken);
        if (hasInvoiceItems)
            throw new BadRequestException("Product cannot be deleted because it has associated invoice items.");

        var deleted = await _productRepository.DeleteAsync(command.Id, cancellationToken);
        if (!deleted)
            throw new BadRequestException("Product deletion failed.");
    }
}
