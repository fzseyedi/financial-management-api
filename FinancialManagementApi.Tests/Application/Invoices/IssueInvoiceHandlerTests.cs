using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Application.Invoices.Commands;
using FinancialManagementApi.Domain.Entities;
using FinancialManagementApi.Domain.Enums;
using FinancialManagementApi.Domain.Exceptions;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace FinancialManagementApi.Tests.Application.Invoices;

public sealed class IssueInvoiceHandlerTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock = new();
    private readonly IValidator<IssueInvoiceCommand> _validator = new IssueInvoiceCommandValidator();

    [Fact]
    public async Task HandleAsync_Should_Issue_Invoice_When_It_Has_Items()
    {
        var invoice = new Invoice("INV-0001", 1, new DateTime(2026, 4, 9), null);

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _invoiceRepositoryMock
            .Setup(x => x.HasItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _invoiceRepositoryMock
            .Setup(x => x.UpdateAsync(invoice, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new IssueInvoiceHandler(_invoiceRepositoryMock.Object, _validator);

        await handler.HandleAsync(new IssueInvoiceCommand(1), CancellationToken.None);

        invoice.Status.Should().Be(InvoiceStatus.Issued);

        _invoiceRepositoryMock.Verify(
            x => x.UpdateAsync(invoice, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Invoice_Does_Not_Exist()
    {
        _invoiceRepositoryMock
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        var handler = new IssueInvoiceHandler(_invoiceRepositoryMock.Object, _validator);

        var act = async () => await handler.HandleAsync(new IssueInvoiceCommand(99), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Invoice with id 99 was not found.");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_InvoiceCannotBeIssuedException_When_Invoice_Has_No_Items()
    {
        var invoice = new Invoice("INV-0001", 1, new DateTime(2026, 4, 9), null);

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _invoiceRepositoryMock
            .Setup(x => x.HasItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new IssueInvoiceHandler(_invoiceRepositoryMock.Object, _validator);

        var act = async () => await handler.HandleAsync(new IssueInvoiceCommand(1), CancellationToken.None);

        await act.Should().ThrowAsync<InvoiceCannotBeIssuedException>();
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_InvalidInvoiceStateException_When_Invoice_Is_Not_Draft()
    {
        var invoice = new Invoice("INV-0001", 1, new DateTime(2026, 4, 9), null);
        invoice.Issue(true);

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _invoiceRepositoryMock
            .Setup(x => x.HasItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new IssueInvoiceHandler(_invoiceRepositoryMock.Object, _validator);

        var act = async () => await handler.HandleAsync(new IssueInvoiceCommand(1), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidInvoiceStateException>()
            .WithMessage("Only draft invoices can be issued.");
    }
}