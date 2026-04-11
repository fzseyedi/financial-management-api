using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Application.Products.Commands.DeactivateProduct;
using FinancialManagementApi.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace FinancialManagementApi.Tests.Application.Products;

public sealed class DeactivateProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly IValidator<DeactivateProductCommand> _validator = new DeactivateProductCommandValidator();

    [Fact]
    public async Task HandleAsync_Should_Deactivate_Product_When_Product_Is_Active()
    {
        var product = new Product("PRD-001", "Office Chair", 150.00m);
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(product, 1);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _productRepositoryMock
            .Setup(x => x.UpdateAsync(product, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeactivateProductHandler(_productRepositoryMock.Object, _validator);

        await handler.HandleAsync(new DeactivateProductCommand(1), CancellationToken.None);

        product.IsActive.Should().BeFalse();

        _productRepositoryMock.Verify(
            x => x.UpdateAsync(product, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Product_Does_Not_Exist()
    {
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new DeactivateProductHandler(_productRepositoryMock.Object, _validator);

        var act = async () => await handler.HandleAsync(new DeactivateProductCommand(99), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Product with id 99 was not found.");
    }

    [Fact]
    public async Task HandleAsync_Should_Not_Update_When_Product_Is_Already_Inactive()
    {
        var product = new Product("PRD-001", "Office Chair", 150.00m);
        product.Deactivate();
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(product, 1);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var handler = new DeactivateProductHandler(_productRepositoryMock.Object, _validator);

        await handler.HandleAsync(new DeactivateProductCommand(1), CancellationToken.None);

        _productRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}