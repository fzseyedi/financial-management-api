using FinancialManagementApi.Application.Products.Commands.DeactivateProduct;
using FluentAssertions;

namespace FinancialManagementApi.Tests.Validators.Products;

public sealed class DeactivateProductCommandValidatorTests
{
    private readonly DeactivateProductCommandValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_Should_Pass_When_Id_Is_Valid()
    {
        var command = new DeactivateProductCommand(1);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Id_Is_Zero()
    {
        var command = new DeactivateProductCommand(0);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Id");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Id_Is_Negative()
    {
        var command = new DeactivateProductCommand(-1);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Id");
    }
}