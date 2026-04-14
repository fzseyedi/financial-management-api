using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Products.Queries;
using FinancialManagementApi.Application.DTOs;
using FinancialManagementApi.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FinancialManagementApi.Tests.Application.Products;

public sealed class GetAllProductsHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();

    [Fact]
    public async Task HandlePagedAsync_Should_Return_Paginated_Response_With_First_Page()
    {
        var products = new List<Product>
        {
            new("PROD-001", "Product A", 100.00m),
            new("PROD-002", "Product B", 200.00m),
            new("PROD-003", "Product C", 300.00m),
            new("PROD-004", "Product D", 400.00m),
            new("PROD-005", "Product E", 500.00m)
        };

        var query = new GetAllProductsPagedQuery(false, 1, 3);

        _productRepositoryMock
            .Setup(x => x.GetAllPagedAsync(
                query.IncludeInactive,
                query.PageNumber,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((products.Take(3).ToList() as IEnumerable<Product>, 5));

        var handler = new GetAllProductsHandler(_productRepositoryMock.Object);

        var result = await handler.HandlePagedAsync(query, CancellationToken.None);

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(3);
        result.TotalPages.Should().Be(2); // 5 total / 3 per page = 2 pages

        _productRepositoryMock.Verify(
            x => x.GetAllPagedAsync(false, 1, 3, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandlePagedAsync_Should_Return_Paginated_Response_With_Second_Page()
    {
        var products = new List<Product>
        {
            new("PROD-004", "Product D", 400.00m),
            new("PROD-005", "Product E", 500.00m)
        };

        var query = new GetAllProductsPagedQuery(false, 2, 3);

        _productRepositoryMock
            .Setup(x => x.GetAllPagedAsync(
                query.IncludeInactive,
                query.PageNumber,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 5));

        var handler = new GetAllProductsHandler(_productRepositoryMock.Object);

        var result = await handler.HandlePagedAsync(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(3);
        result.TotalPages.Should().Be(2);

        _productRepositoryMock.Verify(
            x => x.GetAllPagedAsync(false, 2, 3, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandlePagedAsync_Should_Return_Empty_Items_When_Page_Is_Beyond_Total()
    {
        var query = new GetAllProductsPagedQuery(false, 10, 10);

        _productRepositoryMock
            .Setup(x => x.GetAllPagedAsync(
                query.IncludeInactive,
                query.PageNumber,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Product>() as IEnumerable<Product>, 5));

        var handler = new GetAllProductsHandler(_productRepositoryMock.Object);

        var result = await handler.HandlePagedAsync(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task HandlePagedAsync_Should_Include_Inactive_When_Specified()
    {
        var products = new List<Product>
        {
            new("PROD-001", "Product A", 100.00m),
            new("PROD-002", "Product B", 200.00m),
            new("PROD-003", "Product C", 300.00m)
        };

        var query = new GetAllProductsPagedQuery(true, 1, 10);

        _productRepositoryMock
            .Setup(x => x.GetAllPagedAsync(
                true,
                query.PageNumber,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 3));

        var handler = new GetAllProductsHandler(_productRepositoryMock.Object);

        var result = await handler.HandlePagedAsync(query, CancellationToken.None);

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);

        _productRepositoryMock.Verify(
            x => x.GetAllPagedAsync(true, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandlePagedAsync_Should_Calculate_Total_Pages_Correctly()
    {
        var query = new GetAllProductsPagedQuery(false, 1, 10);

        _productRepositoryMock
            .Setup(x => x.GetAllPagedAsync(
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Product>(), 27));

        var handler = new GetAllProductsHandler(_productRepositoryMock.Object);

        var result = await handler.HandlePagedAsync(query, CancellationToken.None);

        result.TotalPages.Should().Be(3); // 27 / 10 = 2.7 -> 3 pages
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Non_Paginated_Response()
    {
        var products = new List<Product>
        {
            new("PROD-001", "Product A", 100.00m),
            new("PROD-002", "Product B", 200.00m)
        };

        var query = new GetAllProductsQuery(false);

        _productRepositoryMock
            .Setup(x => x.GetAllAsync(query.IncludeInactive, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var handler = new GetAllProductsHandler(_productRepositoryMock.Object);

        var result = await handler.HandleAsync(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result.First().Name.Should().Be("Product A");

        _productRepositoryMock.Verify(
            x => x.GetAllAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
