using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FluentValidation;

namespace FinancialManagementApi.Application.Products.Commands.DeactivateProduct;

public sealed class DeactivateProductHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IValidator<DeactivateProductCommand> _validator;

    public DeactivateProductHandler(
        IProductRepository productRepository,
        IValidator<DeactivateProductCommand> validator)
    {
        _productRepository = productRepository;
        _validator = validator;
    }

    public async Task HandleAsync(DeactivateProductCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var product = await _productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
            throw new NotFoundException($"Product with id {command.Id} was not found.");

        if (!product.IsActive)
            return;

        product.Deactivate();

        var updated = await _productRepository.UpdateAsync(product, cancellationToken);
        if (!updated)
            throw new BadRequestException("Product deactivation failed.");
    }
}