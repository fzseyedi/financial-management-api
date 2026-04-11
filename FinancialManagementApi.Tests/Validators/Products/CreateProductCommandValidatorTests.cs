using FinancialManagementApi.Application.Products.Commands.CreateProdcut;
using FluentAssertions;

namespace FinancialManagementApi.Tests.Validators.Products;

public sealed class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_Should_Pass_When_Command_Is_Valid()
    {
        var command = new CreateProductCommand(
            "PRD-001",
            "Office Chair",
            150.00m);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Code_Is_Empty()
    {
        var command = new CreateProductCommand(
            "",
            "Office Chair",
            150.00m);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Code");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Name_Is_Empty()
    {
        var command = new CreateProductCommand(
            "PRD-001",
            "",
            150.00m);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Name");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_UnitPrice_Is_Zero()
    {
        var command = new CreateProductCommand(
            "PRD-001",
            "Office Chair",
            0m);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "UnitPrice");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_UnitPrice_Is_Negative()
    {
        var command = new CreateProductCommand(
            "PRD-001",
            "Office Chair",
            -10m);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "UnitPrice");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Code_Exceeds_MaxLength()
    {
        var command = new CreateProductCommand(
            new string('A', 21),
            "Office Chair",
            150.00m);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Code");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Name_Exceeds_MaxLength()
    {
        var command = new CreateProductCommand(
            "PRD-001",
            new string('A', 201),
            150.00m);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Name");
    }
}