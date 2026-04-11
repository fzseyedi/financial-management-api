using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Application.Products.Commands.CreateProdcut;
using FinancialManagementApi.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace FinancialManagementApi.Tests.Application.Products;

public sealed class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly IValidator<CreateProductCommand> _validator = new CreateProductCommandValidator();

    [Fact]
    public async Task HandleAsync_Should_Create_Product_When_Command_Is_Valid()
    {
        var command = new CreateProductCommand(
            "PRD-001",
            "Office Chair",
            150.00m);

        _productRepositoryMock
            .Setup(x => x.ExistsByCodeAsync(command.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _productRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        var handler = new CreateProductHandler(_productRepositoryMock.Object, _validator);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Should().Be(10);

        _productRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_ConflictException_When_Code_Already_Exists()
    {
        var command = new CreateProductCommand(
            "PRD-001",
            "Office Chair",
            150.00m);

        _productRepositoryMock
            .Setup(x => x.ExistsByCodeAsync(command.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateProductHandler(_productRepositoryMock.Object, _validator);

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Product code*already exists*");

        _productRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_ValidationException_When_Command_Is_Invalid()
    {
        var command = new CreateProductCommand(
            "",
            "",
            0m);

        var handler = new CreateProductHandler(_productRepositoryMock.Object, _validator);

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();

        _productRepositoryMock.Verify(
            x => x.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}