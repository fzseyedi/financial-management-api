using FinancialManagementApi.Application.Customers.Commands.UpdateCustomer;
using FluentAssertions;

namespace FinancialManagementApi.Tests.Validators.Customers;

public sealed class UpdateCustomerCommandValidatorTests
{
    private readonly UpdateCustomerCommandValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_Should_Pass_When_Command_Is_Valid()
    {
        var command = new UpdateCustomerCommand(
            1,
            "CUST-001",
            "Acme Trading",
            "info@acme.com",
            "111111",
            "Istanbul");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Id_Is_Invalid()
    {
        var command = new UpdateCustomerCommand(
            0,
            "CUST-001",
            "Acme Trading",
            "info@acme.com",
            "111111",
            "Istanbul");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Id");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Code_Is_Empty()
    {
        var command = new UpdateCustomerCommand(
            1,
            "",
            "Acme Trading",
            "info@acme.com",
            "111111",
            "Istanbul");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Code");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Name_Is_Empty()
    {
        var command = new UpdateCustomerCommand(
            1,
            "CUST-001",
            "",
            "info@acme.com",
            "111111",
            "Istanbul");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Name");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Email_Is_Invalid()
    {
        var command = new UpdateCustomerCommand(
            1,
            "CUST-001",
            "Acme Trading",
            "invalid-email",
            "111111",
            "Istanbul");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Email");
    }
}