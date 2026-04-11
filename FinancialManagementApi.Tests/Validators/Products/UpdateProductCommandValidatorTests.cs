using FinancialManagementApi.Application.Products.Commands.UpdateProduct;
using FluentAssertions;

namespace FinancialManagementApi.Tests.Validators.Products;

public sealed class UpdateProductCommandValidatorTests
{
    private readonly UpdateProductCommandValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_Should_Pass_When_Command_Is_Valid()
    {
        var command = new UpdateProductCommand(
            1,
            "PRD-001",
            "Office Chair",
            150.00m);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Id_Is_Invalid()
    {
        var command = new UpdateProductCommand(
            0,
            "PRD-001",
            "Office Chair",
            150.00m);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Id");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Code_Is_Empty()
    {
        var command = new UpdateProductCommand(
            1,
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
        var command = new UpdateProductCommand(
            1,
            "PRD-001",
            "",
            150.00m);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Name");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_UnitPrice_Is_Not_Greater_Than_Zero()
    {
        var command = new UpdateProductCommand(
            1,
            "PRD-001",
            "Office Chair",
            0m);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "UnitPrice");
    }
}