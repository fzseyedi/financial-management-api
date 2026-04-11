using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Domain.Entities;
using FluentValidation;

namespace FinancialManagementApi.Application.Products.Commands.CreateProdcut;

public sealed class CreateProductHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IValidator<CreateProductCommand> _validator;

    public CreateProductHandler(
        IProductRepository productRepository,
        IValidator<CreateProductCommand> validator)
    {
        _productRepository = productRepository;
        _validator = validator;
    }

    public async Task<int> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var exists = await _productRepository.ExistsByCodeAsync(command.Code, cancellationToken);
        if (exists)
            throw new ConflictException($"Product code '{command.Code}' already exists.");

        var product = new Product(
            command.Code.Trim(),
            command.Name.Trim(),
            command.UnitPrice);

        return await _productRepository.CreateAsync(product, cancellationToken);
    }
}