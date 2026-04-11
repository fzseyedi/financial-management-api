using FinancialManagementApi.Application.Customers.Commands.DeactivateCustomer;
using FluentAssertions;

namespace FinancialManagementApi.Tests.Validators.Customers;

public sealed class DeactivateCustomerCommandValidatorTests
{
    private readonly DeactivateCustomerCommandValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_Should_Pass_When_Id_Is_Valid()
    {
        var command = new DeactivateCustomerCommand(1);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Id_Is_Zero()
    {
        var command = new DeactivateCustomerCommand(0);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Id");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Id_Is_Negative()
    {
        var command = new DeactivateCustomerCommand(-1);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Id");
    }
}