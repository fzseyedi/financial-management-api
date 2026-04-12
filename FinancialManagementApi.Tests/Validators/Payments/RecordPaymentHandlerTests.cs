using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Application.Payments.Commands.RecordPayment;
using FinancialManagementApi.Domain.Entities;
using FinancialManagementApi.Domain.Enums;
using FinancialManagementApi.Domain.Exceptions;
using FluentAssertions;
using FluentValidation;
using Moq;
using System.Data;

namespace FinancialManagementApi.Tests.Application.Payments;

public sealed class RecordPaymentHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock = new();
    private readonly Mock<ICustomerRepository> _customerRepositoryMock = new();
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly IValidator<RecordPaymentCommand> _validator = new RecordPaymentCommandValidator();

    [Fact]
    public async Task HandleAsync_Should_Record_Payment_And_Update_Invoice()
    {
        var customer = new Customer("CUST-001", "Acme Trading", "info@acme.com", "111111", "Istanbul");
        var invoice = new Invoice("INV-0001", 1, new DateTime(2026, 4, 9), null);
        invoice.SetTotalAmount(300m);
        invoice.Issue(true);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Mock the UnitOfWork to execute the transaction action
        _unitOfWorkMock
            .Setup(x => x.ExecuteInTransactionAsync(
                It.IsAny<Func<IDbTransaction, Task<int>>>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (Func<IDbTransaction, Task<int>> action, CancellationToken _) =>
            {
                // Execute the action without a real transaction
                var mockTransaction = new Mock<IDbTransaction>();
                return await action(mockTransaction.Object);
            });

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdWithLockAsync(1, It.IsAny<IDbTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _invoiceRepositoryMock
            .Setup(x => x.UpdateAsync(invoice, It.IsAny<IDbTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _paymentRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Payment>(), It.IsAny<IDbTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        var handler = new RecordPaymentHandler(
            _paymentRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _invoiceRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _validator);

        var command = new RecordPaymentCommand(
            1,
            1,
            new DateTime(2026, 4, 10),
            100m,
            "PAY-001",
            "First payment");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Should().Be(10);
        invoice.PaidAmount.Should().Be(100m);
        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);

        _invoiceRepositoryMock.Verify(
            x => x.UpdateAsync(invoice, It.IsAny<IDbTransaction>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _paymentRepositoryMock.Verify(
            x => x.CreateAsync(
                It.Is<Payment>(p =>
                    p.CustomerId == 1 &&
                    p.InvoiceId == 1 &&
                    p.Amount == 100m &&
                    p.ReferenceNumber == "PAY-001" &&
                    p.Notes == "First payment"),
                It.IsAny<IDbTransaction>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Customer_Does_Not_Exist()
    {
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var handler = new RecordPaymentHandler(
            _paymentRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _invoiceRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _validator);

        var command = new RecordPaymentCommand(99, 1, new DateTime(2026, 4, 10), 100m, null, null);

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Customer with id 99 was not found.");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Invoice_Does_Not_Exist()
    {
        var customer = new Customer("CUST-001", "Acme Trading", "info@acme.com", "111111", "Istanbul");

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _unitOfWorkMock
            .Setup(x => x.ExecuteInTransactionAsync(
                It.IsAny<Func<IDbTransaction, Task<int>>>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (Func<IDbTransaction, Task<int>> action, CancellationToken _) =>
            {
                var mockTransaction = new Mock<IDbTransaction>();
                return await action(mockTransaction.Object);
            });

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdWithLockAsync(99, It.IsAny<IDbTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        var handler = new RecordPaymentHandler(
            _paymentRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _invoiceRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _validator);

        var command = new RecordPaymentCommand(1, 99, new DateTime(2026, 4, 10), 100m, null, null);

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Invoice with id 99 was not found.");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_BadRequestException_When_Invoice_Does_Not_Belong_To_Customer()
    {
        var customer = new Customer("CUST-001", "Acme Trading", "info@acme.com", "111111", "Istanbul");
        var invoice = new Invoice("INV-0001", 2, new DateTime(2026, 4, 9), null);
        invoice.SetTotalAmount(300m);
        invoice.Issue(true);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _unitOfWorkMock
            .Setup(x => x.ExecuteInTransactionAsync(
                It.IsAny<Func<IDbTransaction, Task<int>>>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (Func<IDbTransaction, Task<int>> action, CancellationToken _) =>
            {
                var mockTransaction = new Mock<IDbTransaction>();
                return await action(mockTransaction.Object);
            });

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdWithLockAsync(1, It.IsAny<IDbTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var handler = new RecordPaymentHandler(
            _paymentRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _invoiceRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _validator);

        var command = new RecordPaymentCommand(1, 1, new DateTime(2026, 4, 10), 100m, null, null);

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("The specified invoice does not belong to the specified customer.");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_PaymentExceedsInvoiceBalanceException_When_Amount_Exceeds_Remaining()
    {
        var customer = new Customer("CUST-001", "Acme Trading", "info@acme.com", "111111", "Istanbul");
        var invoice = new Invoice("INV-0001", 1, new DateTime(2026, 4, 9), null);
        invoice.SetTotalAmount(300m);
        invoice.Issue(true);
        invoice.RecordPayment(250m);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _unitOfWorkMock
            .Setup(x => x.ExecuteInTransactionAsync(
                It.IsAny<Func<IDbTransaction, Task<int>>>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (Func<IDbTransaction, Task<int>> action, CancellationToken _) =>
            {
                var mockTransaction = new Mock<IDbTransaction>();
                return await action(mockTransaction.Object);
            });

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdWithLockAsync(1, It.IsAny<IDbTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var handler = new RecordPaymentHandler(
            _paymentRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _invoiceRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _validator);

        var command = new RecordPaymentCommand(1, 1, new DateTime(2026, 4, 10), 100m, null, null);

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<PaymentExceedsInvoiceBalanceException>();
    }
}