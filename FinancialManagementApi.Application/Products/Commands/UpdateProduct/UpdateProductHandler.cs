using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FluentValidation;

namespace FinancialManagementApi.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IValidator<UpdateProductCommand> _validator;

    public UpdateProductHandler(
        IProductRepository productRepository,
        IValidator<UpdateProductCommand> validator)
    {
        _productRepository = productRepository;
        _validator = validator;
    }

    public async Task HandleAsync(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var product = await _productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
            throw new NotFoundException($"Product with id {command.Id} was not found.");

        var exists = await _productRepository.ExistsByCodeAsync(command.Code, command.Id, cancellationToken);
        if (exists)
            throw new ConflictException($"Product code '{command.Code}' already exists.");

        product.Update(
            command.Code.Trim(),
            command.Name.Trim(),
            command.UnitPrice);

        var updated = await _productRepository.UpdateAsync(product, cancellationToken);
        if (!updated)
            throw new BadRequestException("Product update failed.");
    }
}