using FinancialManagementApi.Domain.Entities;
using FluentAssertions;

namespace FinancialManagementApi.Tests.Domain.Products;

public sealed class ProductTests
{
    [Fact]
    public void Constructor_Should_Create_Product_With_Expected_Values()
    {
        var product = new Product(
            "PRD-001",
            "Office Chair",
            150.00m);

        product.Code.Should().Be("PRD-001");
        product.Name.Should().Be("Office Chair");
        product.UnitPrice.Should().Be(150.00m);
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Update_Should_Modify_Product_Fields()
    {
        var product = new Product(
            "PRD-001",
            "Office Chair",
            150.00m);

        product.Update(
            "PRD-002",
            "Office Chair Deluxe",
            175.00m);

        product.Code.Should().Be("PRD-002");
        product.Name.Should().Be("Office Chair Deluxe");
        product.UnitPrice.Should().Be(175.00m);
    }

    [Fact]
    public void Deactivate_Should_Set_IsActive_To_False()
    {
        var product = new Product(
            "PRD-001",
            "Office Chair",
            150.00m);

        product.Deactivate();

        product.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_Should_Set_IsActive_To_True()
    {
        var product = new Product(
            "PRD-001",
            "Office Chair",
            150.00m);

        product.Deactivate();
        product.IsActive.Should().BeFalse();

        product.Activate();

        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_Should_Keep_IsActive_True_When_Already_Active()
    {
        var product = new Product(
            "PRD-001",
            "Office Chair",
            150.00m);

        product.Activate();

        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_Should_Keep_IsActive_False_When_Already_Inactive()
    {
        var product = new Product(
            "PRD-001",
            "Office Chair",
            150.00m);

        product.Deactivate();
        product.Deactivate();

        product.IsActive.Should().BeFalse();
    }
}