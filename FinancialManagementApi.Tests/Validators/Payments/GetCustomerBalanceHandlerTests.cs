using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Application.DTOs;
using FinancialManagementApi.Application.Reports.Queries;
using FluentAssertions;
using Moq;

namespace FinancialManagementApi.Tests.Application.Reports;

public sealed class GetCustomerBalanceHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock = new();

    [Fact]
    public async Task HandleAsync_Should_Return_CustomerBalanceDto_When_Found()
    {
        var dto = new CustomerBalanceDto(
            1,
            "Acme Trading",
            345m,
            100m,
            245m);

        _paymentRepositoryMock
            .Setup(x => x.GetCustomerBalanceAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var handler = new GetCustomerBalanceHandler(_paymentRepositoryMock.Object);

        var result = await handler.HandleAsync(new GetCustomerBalanceQuery(1), CancellationToken.None);

        result.CustomerId.Should().Be(1);
        result.CustomerName.Should().Be("Acme Trading");
        result.Balance.Should().Be(245m);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Customer_Is_Not_Found()
    {
        _paymentRepositoryMock
            .Setup(x => x.GetCustomerBalanceAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerBalanceDto?)null);

        var handler = new GetCustomerBalanceHandler(_paymentRepositoryMock.Object);

        var act = async () => await handler.HandleAsync(new GetCustomerBalanceQuery(99), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Customer with id 99 was not found.");
    }
}