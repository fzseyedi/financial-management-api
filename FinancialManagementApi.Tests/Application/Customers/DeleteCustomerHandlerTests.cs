using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Application.Customers.Commands.DeleteCustomer;
using FinancialManagementApi.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FinancialManagementApi.Tests.Application.Customers;

public sealed class DeleteCustomerHandlerTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock = new();
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock = new();
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock = new();

    [Fact]
    public async Task HandleAsync_Should_Delete_Customer_When_No_Invoices_Or_Payments_Exist()
    {
        var customer = new Customer("CUST-005", "Acme Trading", "info@acme.com", "111111", "Istanbul");
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(customer, 1005);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(1005, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _invoiceRepositoryMock
            .Setup(x => x.HasCustomerInvoicesAsync(1005, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _paymentRepositoryMock
            .Setup(x => x.HasCustomerPaymentsAsync(1005, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _customerRepositoryMock
            .Setup(x => x.DeleteAsync(1005, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeleteCustomerHandler(
            _customerRepositoryMock.Object,
            _invoiceRepositoryMock.Object,
            _paymentRepositoryMock.Object);

        await handler.HandleAsync(new DeleteCustomerCommand(1005), CancellationToken.None);

        _customerRepositoryMock.Verify(
            x => x.DeleteAsync(1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Customer_Does_Not_Exist()
    {
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var handler = new DeleteCustomerHandler(
            _customerRepositoryMock.Object,
            _invoiceRepositoryMock.Object,
            _paymentRepositoryMock.Object);

        var act = async () => await handler.HandleAsync(new DeleteCustomerCommand(99), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Customer with id 99 was not found.");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_ConflictException_When_Customer_Has_Invoices()
    {
        var customer = new Customer("CUST-001", "Acme Trading", "info@acme.com", "111111", "Istanbul");
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(customer, 1);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _invoiceRepositoryMock
            .Setup(x => x.HasCustomerInvoicesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeleteCustomerHandler(
            _customerRepositoryMock.Object,
            _invoiceRepositoryMock.Object,
            _paymentRepositoryMock.Object);

        var act = async () => await handler.HandleAsync(new DeleteCustomerCommand(1), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Customer cannot be deleted because they have associated invoices.");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_ConflictException_When_Customer_Has_Payments()
    {
        var customer = new Customer("CUST-001", "Acme Trading", "info@acme.com", "111111", "Istanbul");
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(customer, 1);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _invoiceRepositoryMock
            .Setup(x => x.HasCustomerInvoicesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _paymentRepositoryMock
            .Setup(x => x.HasCustomerPaymentsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeleteCustomerHandler(
            _customerRepositoryMock.Object,
            _invoiceRepositoryMock.Object,
            _paymentRepositoryMock.Object);

        var act = async () => await handler.HandleAsync(new DeleteCustomerCommand(1), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Customer cannot be deleted because they have associated payments.");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_BadRequestException_When_Delete_Fails()
    {
        var customer = new Customer("CUST-001", "Acme Trading", "info@acme.com", "111111", "Istanbul");
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(customer, 1);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _invoiceRepositoryMock
            .Setup(x => x.HasCustomerInvoicesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _paymentRepositoryMock
            .Setup(x => x.HasCustomerPaymentsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _customerRepositoryMock
            .Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new DeleteCustomerHandler(
            _customerRepositoryMock.Object,
            _invoiceRepositoryMock.Object,
            _paymentRepositoryMock.Object);

        var act = async () => await handler.HandleAsync(new DeleteCustomerCommand(1), CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Customer deletion failed.");
    }
}
