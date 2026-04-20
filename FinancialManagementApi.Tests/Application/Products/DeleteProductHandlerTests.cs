using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Application.Products.Commands.DeleteProduct;
using FinancialManagementApi.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FinancialManagementApi.Tests.Application.Products;

public sealed class DeleteProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock = new();

    [Fact]
    public async Task HandleAsync_Should_Delete_Product_When_No_Invoice_Items_Exist()
    {
        var product = new Product("PRD-001", "Office Chair", 150.00m);
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(product, 1);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _invoiceRepositoryMock
            .Setup(x => x.HasProductInvoiceItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _productRepositoryMock
            .Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeleteProductHandler(
            _productRepositoryMock.Object,
            _invoiceRepositoryMock.Object);

        await handler.HandleAsync(new DeleteProductCommand(1), CancellationToken.None);

        _productRepositoryMock.Verify(
            x => x.DeleteAsync(1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Product_Does_Not_Exist()
    {
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new DeleteProductHandler(
            _productRepositoryMock.Object,
            _invoiceRepositoryMock.Object);

        var act = async () => await handler.HandleAsync(new DeleteProductCommand(99), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Product with id 99 was not found.");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_BadRequestException_When_Product_Has_Invoice_Items()
    {
        var product = new Product("PRD-001", "Office Chair", 150.00m);
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(product, 1);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _invoiceRepositoryMock
            .Setup(x => x.HasProductInvoiceItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeleteProductHandler(
            _productRepositoryMock.Object,
            _invoiceRepositoryMock.Object);

        var act = async () => await handler.HandleAsync(new DeleteProductCommand(1), CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Product cannot be deleted because it has associated invoice items.");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_BadRequestException_When_Delete_Fails()
    {
        var product = new Product("PRD-001", "Office Chair", 150.00m);
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(product, 1);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _invoiceRepositoryMock
            .Setup(x => x.HasProductInvoiceItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _productRepositoryMock
            .Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new DeleteProductHandler(
            _productRepositoryMock.Object,
            _invoiceRepositoryMock.Object);

        var act = async () => await handler.HandleAsync(new DeleteProductCommand(1), CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Product deletion failed.");
    }
}
