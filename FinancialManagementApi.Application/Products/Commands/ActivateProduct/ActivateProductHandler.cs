using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FluentValidation;

namespace FinancialManagementApi.Application.Products.Commands.ActivateProduct;

public sealed class ActivateProductHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IValidator<ActivateProductCommand> _validator;

    public ActivateProductHandler(
        IProductRepository productRepository,
        IValidator<ActivateProductCommand> validator)
    {
        _productRepository = productRepository;
        _validator = validator;
    }

    public async Task HandleAsync(ActivateProductCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var product = await _productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
            throw new NotFoundException($"Product with id {command.Id} was not found.");

        if (product.IsActive)
            return;

        product.Activate();

        var updated = await _productRepository.UpdateAsync(product, cancellationToken);
        if (!updated)
            throw new BadRequestException("Product activation failed.");
    }
}