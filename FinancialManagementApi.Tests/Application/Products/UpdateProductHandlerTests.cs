using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Application.Products.Commands.UpdateProduct;
using FinancialManagementApi.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace FinancialManagementApi.Tests.Application.Products;

public sealed class UpdateProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly IValidator<UpdateProductCommand> _validator = new UpdateProductCommandValidator();

    [Fact]
    public async Task HandleAsync_Should_Update_Product_When_Command_Is_Valid()
    {
        var product = new Product("PRD-001", "Office Chair", 150.00m);
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(product, 1);

        var command = new UpdateProductCommand(
            1,
            "PRD-002",
            "Office Chair Deluxe",
            175.00m);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _productRepositoryMock
            .Setup(x => x.ExistsByCodeAsync(command.Code, command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _productRepositoryMock
            .Setup(x => x.UpdateAsync(product, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new UpdateProductHandler(_productRepositoryMock.Object, _validator);

        await handler.HandleAsync(command, CancellationToken.None);

        product.Code.Should().Be("PRD-002");
        product.Name.Should().Be("Office Chair Deluxe");
        product.UnitPrice.Should().Be(175.00m);

        _productRepositoryMock.Verify(
            x => x.UpdateAsync(product, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Product_Does_Not_Exist()
    {
        var command = new UpdateProductCommand(
            99,
            "PRD-001",
            "Office Chair",
            150.00m);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new UpdateProductHandler(_productRepositoryMock.Object, _validator);

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Product with id 99 was not found.");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_ConflictException_When_Code_Already_Exists()
    {
        var product = new Product("PRD-001", "Office Chair", 150.00m);
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(product, 1);

        var command = new UpdateProductCommand(
            1,
            "PRD-002",
            "Office Chair Deluxe",
            175.00m);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _productRepositoryMock
            .Setup(x => x.ExistsByCodeAsync(command.Code, command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new UpdateProductHandler(_productRepositoryMock.Object, _validator);

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Product code*already exists*");
    }
}