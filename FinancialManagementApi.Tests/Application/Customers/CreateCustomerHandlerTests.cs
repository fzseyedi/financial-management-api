using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Application.Customers.Commands.CreateCustomer;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace FinancialManagementApi.Tests.Application.Customers;

public sealed class CreateCustomerHandlerTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock = new();
    private readonly IValidator<CreateCustomerCommand> _validator = new CreateCustomerCommandValidator();

    [Fact]
    public async Task HandleAsync_Should_Create_Customer_When_Command_Is_Valid()
    {
        var command = new CreateCustomerCommand(
            "CUST-001",
            "Acme Trading",
            "info@acme.com",
            "111111",
            "Istanbul");

        _customerRepositoryMock
            .Setup(x => x.ExistsByCodeAsync(command.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _customerRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<FinancialManagementApi.Domain.Entities.Customer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        var handler = new CreateCustomerHandler(_customerRepositoryMock.Object, _validator);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Should().Be(10);

        _customerRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<FinancialManagementApi.Domain.Entities.Customer>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_ConflictException_When_Code_Already_Exists()
    {
        var command = new CreateCustomerCommand(
            "CUST-001",
            "Acme Trading",
            "info@acme.com",
            "111111",
            "Istanbul");

        _customerRepositoryMock
            .Setup(x => x.ExistsByCodeAsync(command.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateCustomerHandler(_customerRepositoryMock.Object, _validator);

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Customer code*already exists*");

        _customerRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<FinancialManagementApi.Domain.Entities.Customer>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_ValidationException_When_Command_Is_Invalid()
    {
        var command = new CreateCustomerCommand(
            "",
            "",
            "invalid-email",
            "111111",
            "Istanbul");

        var handler = new CreateCustomerHandler(_customerRepositoryMock.Object, _validator);

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();

        _customerRepositoryMock.Verify(
            x => x.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}