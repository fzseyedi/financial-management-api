using FinancialManagementApi.Application.Invoices.Commands;
using FinancialManagementApi.Application.Invoices.Commands.CreateInvoice;
using FluentAssertions;

namespace FinancialManagementApi.Tests.Validators.Invoices;

public sealed class CreateInvoiceCommandValidatorTests
{
    private readonly CreateInvoiceCommandValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_Should_Pass_When_Command_Is_Valid()
    {
        var command = new CreateInvoiceCommand(
            1,
            new DateTime(2026, 4, 9),
            "First invoice",
            new List<CreateInvoiceCommandItem>
            {
                new(1, 2m, 150m),
                new(2, 1m, 45m)
            });

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_CustomerId_Is_Invalid()
    {
        var command = new CreateInvoiceCommand(
            0,
            new DateTime(2026, 4, 9),
            "First invoice",
            new List<CreateInvoiceCommandItem>
            {
                new(1, 2m, 150m)
            });

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "CustomerId");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_InvoiceDate_Is_Empty()
    {
        var command = new CreateInvoiceCommand(
            1,
            default,
            "First invoice",
            new List<CreateInvoiceCommandItem>
            {
                new(1, 2m, 150m)
            });

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "InvoiceDate");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Notes_Exceed_MaxLength()
    {
        var command = new CreateInvoiceCommand(
            1,
            new DateTime(2026, 4, 9),
            new string('A', 501),
            new List<CreateInvoiceCommandItem>
            {
                new(1, 2m, 150m)
            });

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Notes");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Items_Is_Empty()
    {
        var command = new CreateInvoiceCommand(
            1,
            new DateTime(2026, 4, 9),
            "First invoice",
            new List<CreateInvoiceCommandItem>());

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Items");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Item_ProductId_Is_Invalid()
    {
        var command = new CreateInvoiceCommand(
            1,
            new DateTime(2026, 4, 9),
            "First invoice",
            new List<CreateInvoiceCommandItem>
            {
                new(0, 2m, 150m)
            });

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Items[0].ProductId");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Item_Quantity_Is_Not_Greater_Than_Zero()
    {
        var command = new CreateInvoiceCommand(
            1,
            new DateTime(2026, 4, 9),
            "First invoice",
            new List<CreateInvoiceCommandItem>
            {
                new(1, 0m, 150m)
            });

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Items[0].Quantity");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Item_UnitPrice_Is_Not_Greater_Than_Zero()
    {
        var command = new CreateInvoiceCommand(
            1,
            new DateTime(2026, 4, 9),
            "First invoice",
            new List<CreateInvoiceCommandItem>
            {
                new(1, 2m, 0m)
            });

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Items[0].UnitPrice");
    }
}