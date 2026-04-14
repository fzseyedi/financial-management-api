using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.DTOs;
using FinancialManagementApi.Application.Invoices.Queries;
using FluentAssertions;
using Moq;

namespace FinancialManagementApi.Tests.Application.Invoices;

public sealed class GetAllInvoicesHandlerTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock = new();

    [Fact]
    public async Task HandlePagedAsync_Should_Return_Paginated_Response_With_First_Page()
    {
        var invoices = new List<InvoiceSummaryDto>
        {
            new(1, "INV-001", 1, "Customer A", DateTime.Now, 1, 1000m, 0m, 1000m, "Notes", null),
            new(2, "INV-002", 1, "Customer A", DateTime.Now, 1, 2000m, 0m, 2000m, "Notes", null),
            new(3, "INV-003", 2, "Customer B", DateTime.Now, 1, 1500m, 500m, 1000m, "Notes", null)
        };

        var query = new GetAllInvoicesPagedQuery(null, false, null, null, 1, 3);

        _invoiceRepositoryMock
            .Setup(x => x.GetAllPagedAsync(
                null,
                false,
                null,
                null,
                1,
                3,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((invoices, 5));

        var handler = new GetAllInvoicesHandler(_invoiceRepositoryMock.Object);

        var result = await handler.HandlePagedAsync(query, CancellationToken.None);

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(3);
        result.TotalPages.Should().Be(2);

        _invoiceRepositoryMock.Verify(
            x => x.GetAllPagedAsync(null, false, null, null, 1, 3, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandlePagedAsync_Should_Return_Paginated_Response_With_Second_Page()
    {
        var invoices = new List<InvoiceSummaryDto>
        {
            new(4, "INV-004", 2, "Customer B", DateTime.Now, 1, 800m, 0m, 800m, "Notes", null),
            new(5, "INV-005", 3, "Customer C", DateTime.Now, 1, 500m, 0m, 500m, "Notes", null)
        };

        var query = new GetAllInvoicesPagedQuery(null, false, null, null, 2, 3);

        _invoiceRepositoryMock
            .Setup(x => x.GetAllPagedAsync(
                null,
                false,
                null,
                null,
                2,
                3,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((invoices, 5));

        var handler = new GetAllInvoicesHandler(_invoiceRepositoryMock.Object);

        var result = await handler.HandlePagedAsync(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(3);
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task HandlePagedAsync_Should_Return_Empty_Items_When_Page_Is_Beyond_Total()
    {
        var query = new GetAllInvoicesPagedQuery(null, false, null, null, 10, 10);

        _invoiceRepositoryMock
            .Setup(x => x.GetAllPagedAsync(
                null,
                false,
                null,
                null,
                10,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<InvoiceSummaryDto>(), 5));

        var handler = new GetAllInvoicesHandler(_invoiceRepositoryMock.Object);

        var result = await handler.HandlePagedAsync(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task HandlePagedAsync_Should_Filter_By_CustomerId()
    {
        var invoices = new List<InvoiceSummaryDto>
        {
            new(1, "INV-001", 1, "Customer A", DateTime.Now, 1, 1000m, 0m, 1000m, "Notes", null),
            new(2, "INV-002", 1, "Customer A", DateTime.Now, 1, 2000m, 0m, 2000m, "Notes", null)
        };

        var customerId = 1;
        var query = new GetAllInvoicesPagedQuery(customerId, false, null, null, 1, 10);

        _invoiceRepositoryMock
            .Setup(x => x.GetAllPagedAsync(
                customerId,
                false,
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((invoices, 2));

        var handler = new GetAllInvoicesHandler(_invoiceRepositoryMock.Object);

        var result = await handler.HandlePagedAsync(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.All(x => x.CustomerId == customerId).Should().BeTrue();

        _invoiceRepositoryMock.Verify(
            x => x.GetAllPagedAsync(customerId, false, null, null, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandlePagedAsync_Should_Include_Issued_When_Specified()
    {
        var invoices = new List<InvoiceSummaryDto>
        {
            new(1, "INV-001", 1, "Customer A", DateTime.Now, 1, 1000m, 0m, 1000m, "Notes", null),
            new(2, "INV-002", 1, "Customer A", DateTime.Now, 0, 2000m, 0m, 2000m, "Notes", null)
        };

        var query = new GetAllInvoicesPagedQuery(null, true, null, null, 1, 10);

        _invoiceRepositoryMock
            .Setup(x => x.GetAllPagedAsync(
                null,
                true,
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((invoices, 2));

        var handler = new GetAllInvoicesHandler(_invoiceRepositoryMock.Object);

        var result = await handler.HandlePagedAsync(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);

        _invoiceRepositoryMock.Verify(
            x => x.GetAllPagedAsync(null, true, null, null, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandlePagedAsync_Should_Filter_By_Date_Range()
    {
        var dateFrom = new DateTime(2024, 1, 1);
        var dateTo = new DateTime(2024, 1, 31);

        var invoices = new List<InvoiceSummaryDto>
        {
            new(1, "INV-001", 1, "Customer A", new DateTime(2024, 1, 15), 1, 1000m, 0m, 1000m, "Notes", null)
        };

        var query = new GetAllInvoicesPagedQuery(null, false, dateFrom, dateTo, 1, 10);

        _invoiceRepositoryMock
            .Setup(x => x.GetAllPagedAsync(
                null,
                false,
                dateFrom,
                dateTo,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((invoices, 1));

        var handler = new GetAllInvoicesHandler(_invoiceRepositoryMock.Object);

        var result = await handler.HandlePagedAsync(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items.First().InvoiceDate.Should().Be(new DateTime(2024, 1, 15));

        _invoiceRepositoryMock.Verify(
            x => x.GetAllPagedAsync(null, false, dateFrom, dateTo, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandlePagedAsync_Should_Calculate_Total_Pages_Correctly()
    {
        var query = new GetAllInvoicesPagedQuery(null, false, null, null, 1, 10);

        _invoiceRepositoryMock
            .Setup(x => x.GetAllPagedAsync(
                It.IsAny<int?>(),
                It.IsAny<bool>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<InvoiceSummaryDto>(), 27));

        var handler = new GetAllInvoicesHandler(_invoiceRepositoryMock.Object);

        var result = await handler.HandlePagedAsync(query, CancellationToken.None);

        result.TotalPages.Should().Be(3); // 27 / 10 = 2.7 -> 3 pages
    }
}
