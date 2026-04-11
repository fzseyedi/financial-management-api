using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.DTOs;
using FinancialManagementApi.Application.Reports.Queries;
using FinancialManagementApi.Application.Reports.Queries.GetUnpaidInvoices;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialManagementApi.Tests.Validators.Payments;

public sealed class GetUnpaidInvoicesHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock = new();

    [Fact]
    public async Task HandlerAsync_Should_Return_UnpaidInvoices()
    {
        var invoices = new List<UnpaidInvoiceDto>
        {
            new(
                1,
                "INV-0001",
                10,
                "Acme Trading",
                new DateTime(2026, 4, 9),
                300m,
                100m,
                200m,
                2),
            new(
                2,
                "INV-0002",
                11,
                "Blue Star Ltd",
                new DateTime(2026, 4, 10),
                500m,
                0m,
                500m,
                1)
        };

        _paymentRepositoryMock
            .Setup(x => x.GetUnpaidInvoicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoices);

        var handler = new GetUnpaidInvoicesHandler(_paymentRepositoryMock.Object);

        var result = (await handler.HandleAsync(new GetUnpaidInvoicesQuery(), CancellationToken.None)).ToList();

        result.Should().HaveCount(2);
        result[0].InvoiceNumber.Should().Be("INV-0001");
        result[1].InvoiceNumber.Should().Be("INV-0002");

    }

    [Fact]
    public async Task HandlerAsync_Should_Return_Empty_List_When_No_Unpaid_Invoices()
    {
        _paymentRepositoryMock
            .Setup(x => x.GetUnpaidInvoicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UnpaidInvoiceDto>());

        var handler = new GetUnpaidInvoicesHandler(_paymentRepositoryMock.Object);

        var result = (await handler.HandleAsync(new GetUnpaidInvoicesQuery(), CancellationToken.None)).ToList();

        result.Should().BeEmpty();
    }
}
