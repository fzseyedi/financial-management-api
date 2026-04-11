using FinancialManagementApi.Domain.Entities;
using FluentAssertions;

namespace FinancialManagementApi.Tests.Domain.Customers;

public sealed class CustomerTests
{
    [Fact]
    public void Constructor_Should_Create_Customer_With_Expected_Values()
    {
        var customer = new Customer(
            "CUST-001",
            "Acme Trading",
            "info@acme.com",
            "111111",
            "Istanbul");

        customer.Code.Should().Be("CUST-001");
        customer.Name.Should().Be("Acme Trading");
        customer.Email.Should().Be("info@acme.com");
        customer.Phone.Should().Be("111111");
        customer.Address.Should().Be("Istanbul");
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Update_Should_Modify_Customer_Fields()
    {
        var customer = new Customer(
            "CUST-001",
            "Acme Trading",
            "info@acme.com",
            "111111",
            "Istanbul");

        customer.Update(
            "CUST-002",
            "Blue Star Ltd",
            "contact@bluestar.com",
            "222222",
            "Ankara");

        customer.Code.Should().Be("CUST-002");
        customer.Name.Should().Be("Blue Star Ltd");
        customer.Email.Should().Be("contact@bluestar.com");
        customer.Phone.Should().Be("222222");
        customer.Address.Should().Be("Ankara");
    }

    [Fact]
    public void Deactivate_Should_Set_IsActive_To_False()
    {
        var customer = new Customer(
            "CUST-001",
            "Acme Trading",
            "info@acme.com",
            "111111",
            "Istanbul");

        customer.Deactivate();

        customer.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_Should_Set_IsActive_To_True()
    {
        var customer = new Customer(
            "CUST-001",
            "Acme Trading",
            "info@acme.com",
            "111111",
            "Istanbul");

        customer.Deactivate();
        customer.IsActive.Should().BeFalse();

        customer.Activate();

        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_Should_Keep_IsActive_True_When_Already_Active()
    {
        var customer = new Customer(
            "CUST-001",
            "Acme Trading",
            "info@acme.com",
            "111111",
            "Istanbul");

        customer.Activate();

        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_Should_Keep_IsActive_False_When_Already_Inactive()
    {
        var customer = new Customer(
            "CUST-001",
            "Acme Trading",
            "info@acme.com",
            "111111",
            "Istanbul");

        customer.Deactivate();
        customer.Deactivate();

        customer.IsActive.Should().BeFalse();
    }
}