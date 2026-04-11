using FinancialManagementApi.Application.Customers.Commands.CreateCustomer;
using FluentAssertions;

namespace FinancialManagementApi.Tests.Validators.Customers;

public sealed class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_Should_Pass_When_Command_Is_Valid()
    {
        var command = new CreateCustomerCommand(
            "CUST-001",
            "Acme Trading",
            "info@acme.com",
            "111111",
            "Istanbul");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Code_Is_Empty()
    {
        var command = new CreateCustomerCommand(
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
        var command = new CreateCustomerCommand(
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
        var command = new CreateCustomerCommand(
            "CUST-001",
            "Acme Trading",
            "not-a-valid-email",
            "111111",
            "Istanbul");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Email");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Code_Exceeds_MaxLength()
    {
        var command = new CreateCustomerCommand(
            new string('A', 21),
            "Acme Trading",
            "info@acme.com",
            "111111",
            "Istanbul");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Code");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Name_Exceeds_MaxLength()
    {
        var command = new CreateCustomerCommand(
            "CUST-001",
            new string('A', 201),
            "info@acme.com",
            "111111",
            "Istanbul");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Name");
    }
}