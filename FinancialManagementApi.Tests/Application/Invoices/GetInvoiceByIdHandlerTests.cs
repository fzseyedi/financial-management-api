using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.DTOs;
using FinancialManagementApi.Application.Invoices.Queries;
using FluentAssertions;
using Moq;

namespace FinancialManagementApi.Tests.Application.Invoices;

public sealed class GetInvoiceByIdHandlerTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock = new();

    [Fact]
    public async Task HandleAsync_Should_Return_InvoiceDto_When_Invoice_Exists()
    {
        var dto = new InvoiceDto(
            1,
            "INV-0001",
            10,
            "Acme Trading",
            new DateTime(2026, 4, 9),
            1,
            300m,
            0m,
            300m,
            "First invoice",
            new List<InvoiceItemDto>
            {
                new(1, "Office Chair", 2m, 150m, 300m)
            });

        _invoiceRepositoryMock
            .Setup(x => x.GetDetailsByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var handler = new GetInvoiceByIdHandler(_invoiceRepositoryMock.Object);

        var result = await handler.HandleAsync(new GetInvoiceByIdQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.InvoiceNumber.Should().Be("INV-0001");
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Null_When_Invoice_Does_Not_Exist()
    {
        _invoiceRepositoryMock
            .Setup(x => x.GetDetailsByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InvoiceDto?)null);

        var handler = new GetInvoiceByIdHandler(_invoiceRepositoryMock.Object);

        var result = await handler.HandleAsync(new GetInvoiceByIdQuery(99), CancellationToken.None);

        result.Should().BeNull();
    }
}