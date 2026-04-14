using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Customers.Queries;
using FinancialManagementApi.Application.DTOs;
using FinancialManagementApi.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FinancialManagementApi.Tests.Application.Customers;

public sealed class GetAllCustomersHandlerTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock = new();

    [Fact]
    public async Task HandlePagedAsync_Should_Return_Paginated_Response_With_First_Page()
    {
        var customers = new List<Customer>
        {
            new("CUST-001", "Acme Trading", "info@acme.com", null, null),
            new("CUST-002", "Beta Corp", "contact@beta.com", null, null),
            new("CUST-003", "Gamma Inc", "hello@gamma.com", null, null),
            new("CUST-004", "Delta Ltd", "info@delta.com", null, null),
            new("CUST-005", "Epsilon Co", "contact@epsilon.com", null, null)
        };

        var query = new GetAllCustomersPagedQuery(false, 1, 3);

        _customerRepositoryMock
            .Setup(x => x.GetAllPagedAsync(
                query.IncludeInactive,
                query.PageNumber,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((customers.Take(3).ToList() as IEnumerable<Customer>, 5));

        var handler = new GetAllCustomersHandler(_customerRepositoryMock.Object);

        var result = await handler.HandlePagedAsync(query, CancellationToken.None);

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(3);
        result.TotalPages.Should().Be(2); // 5 total / 3 per page = 2 pages

        _customerRepositoryMock.Verify(
            x => x.GetAllPagedAsync(false, 1, 3, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandlePagedAsync_Should_Return_Paginated_Response_With_Second_Page()
    {
        var customers = new List<Customer>
        {
            new("CUST-004", "Delta Ltd", "info@delta.com", null, null),
            new("CUST-005", "Epsilon Co", "contact@epsilon.com", null, null)
        };

        var query = new GetAllCustomersPagedQuery(false, 2, 3);

        _customerRepositoryMock
            .Setup(x => x.GetAllPagedAsync(
                query.IncludeInactive,
                query.PageNumber,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((customers, 5));

        var handler = new GetAllCustomersHandler(_customerRepositoryMock.Object);

        var result = await handler.HandlePagedAsync(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(3);
        result.TotalPages.Should().Be(2);

        _customerRepositoryMock.Verify(
            x => x.GetAllPagedAsync(false, 2, 3, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandlePagedAsync_Should_Return_Empty_Items_When_Page_Is_Beyond_Total()
    {
        var query = new GetAllCustomersPagedQuery(false, 10, 10);

        _customerRepositoryMock
            .Setup(x => x.GetAllPagedAsync(
                query.IncludeInactive,
                query.PageNumber,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Customer>() as IEnumerable<Customer>, 5));

        var handler = new GetAllCustomersHandler(_customerRepositoryMock.Object);

        var result = await handler.HandlePagedAsync(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task HandlePagedAsync_Should_Include_Inactive_When_Specified()
    {
        var customers = new List<Customer>
        {
            new("CUST-001", "Acme Trading", null, null, null),
            new("CUST-002", "Beta Corp", null, null, null),
            new("CUST-003", "Gamma Inc", null, null, null)
        };

        var query = new GetAllCustomersPagedQuery(true, 1, 10);

        _customerRepositoryMock
            .Setup(x => x.GetAllPagedAsync(
                true,
                query.PageNumber,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((customers, 3));

        var handler = new GetAllCustomersHandler(_customerRepositoryMock.Object);

        var result = await handler.HandlePagedAsync(query, CancellationToken.None);

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);

        _customerRepositoryMock.Verify(
            x => x.GetAllPagedAsync(true, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandlePagedAsync_Should_Calculate_Total_Pages_Correctly()
    {
        var query = new GetAllCustomersPagedQuery(false, 1, 10);

        _customerRepositoryMock
            .Setup(x => x.GetAllPagedAsync(
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Customer>(), 27));

        var handler = new GetAllCustomersHandler(_customerRepositoryMock.Object);

        var result = await handler.HandlePagedAsync(query, CancellationToken.None);

        result.TotalPages.Should().Be(3); // 27 / 10 = 2.7 -> 3 pages
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Non_Paginated_Response()
    {
        var customers = new List<Customer>
        {
            new("CUST-001", "Acme Trading", "info@acme.com", null, null),
            new("CUST-002", "Beta Corp", "contact@beta.com", null, null)
        };

        var query = new GetAllCustomersQuery(false);

        _customerRepositoryMock
            .Setup(x => x.GetAllAsync(query.IncludeInactive, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers);

        var handler = new GetAllCustomersHandler(_customerRepositoryMock.Object);

        var result = await handler.HandleAsync(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result.First().Name.Should().Be("Acme Trading");

        _customerRepositoryMock.Verify(
            x => x.GetAllAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
