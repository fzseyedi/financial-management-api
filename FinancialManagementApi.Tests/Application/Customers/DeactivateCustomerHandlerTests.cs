using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Application.Customers.Commands.DeactivateCustomer;
using FinancialManagementApi.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace FinancialManagementApi.Tests.Application.Customers;

public sealed class DeactivateCustomerHandlerTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock = new();
    private readonly IValidator<DeactivateCustomerCommand> _validator = new DeactivateCustomerCommandValidator();

    [Fact]
    public async Task HandleAsync_Should_Deactivate_Customer_When_Customer_Is_Active()
    {
        var customer = new Customer("CUST-001", "Acme Trading", "info@acme.com", "111111", "Istanbul");
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(customer, 1);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _customerRepositoryMock
            .Setup(x => x.UpdateAsync(customer, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeactivateCustomerHandler(_customerRepositoryMock.Object, _validator);

        await handler.HandleAsync(new DeactivateCustomerCommand(1), CancellationToken.None);

        customer.IsActive.Should().BeFalse();

        _customerRepositoryMock.Verify(
            x => x.UpdateAsync(customer, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Customer_Does_Not_Exist()
    {
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var handler = new DeactivateCustomerHandler(_customerRepositoryMock.Object, _validator);

        var act = async () => await handler.HandleAsync(new DeactivateCustomerCommand(99), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Customer with id 99 was not found.");
    }

    [Fact]
    public async Task HandleAsync_Should_Not_Update_When_Customer_Is_Already_Inactive()
    {
        var customer = new Customer("CUST-001", "Acme Trading", "info@acme.com", "111111", "Istanbul");
        customer.Deactivate();
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(customer, 1);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var handler = new DeactivateCustomerHandler(_customerRepositoryMock.Object, _validator);

        await handler.HandleAsync(new DeactivateCustomerCommand(1), CancellationToken.None);

        _customerRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}