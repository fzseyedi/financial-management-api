using FinancialManagementApi.Application.Invoices.Commands.UpdateInvoice;
using FluentAssertions;

namespace FinancialManagementApi.Tests.Validators.Invoices;

public sealed class UpdateInvoiceCommandValidatorTests
{
    private readonly UpdateInvoiceCommandValidator _validator = new();

    [Fact]
    public async Task Validate_Should_Succeed_When_Valid_Command_Provided()
    {
        // Arrange
        var command = new UpdateInvoiceCommand(
            1,
            10,
            DateTime.UtcNow.AddDays(1),
            "Test notes",
            new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 },
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 2m, 150m)
            });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_Should_Fail_When_InvoiceId_Is_Zero()
    {
        // Arrange
        var command = new UpdateInvoiceCommand(
            0,
            10,
            DateTime.UtcNow,
            "Notes",
            new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 },
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 2m, 150m)
            });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "InvoiceId");
    }

    [Fact]
    public async Task Validate_Should_Fail_When_CustomerId_Is_Zero()
    {
        // Arrange
        var command = new UpdateInvoiceCommand(
            1,
            0,
            DateTime.UtcNow,
            "Notes",
            new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 },
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 2m, 150m)
            });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "CustomerId");
    }

    [Fact]
    public async Task Validate_Should_Fail_When_InvoiceDate_Is_Empty()
    {
        // Arrange
        var command = new UpdateInvoiceCommand(
            1,
            10,
            default,
            "Notes",
            new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 },
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 2m, 150m)
            });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "InvoiceDate");
    }

    [Fact]
    public async Task Validate_Should_Fail_When_Notes_Exceeds_MaxLength()
    {
        // Arrange
        var command = new UpdateInvoiceCommand(
            1,
            10,
            DateTime.UtcNow,
            new string('x', 501),
            new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 },
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 2m, 150m)
            });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Notes");
    }

    [Fact]
    public async Task Validate_Should_Fail_When_Version_Is_Empty()
    {
        // Arrange
        var command = new UpdateInvoiceCommand(
            1,
            10,
            DateTime.UtcNow,
            "Notes",
            [],
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 2m, 150m)
            });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Version");
    }

    [Fact]
    public async Task Validate_Should_Fail_When_Items_Is_Empty()
    {
        // Arrange
        var command = new UpdateInvoiceCommand(
            1,
            10,
            DateTime.UtcNow,
            "Notes",
            new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 },
            "TestUser",
            new List<UpdateInvoiceCommandItem>());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Items");
    }

    [Fact]
    public async Task Validate_Should_Fail_When_Item_ProductId_Is_Zero()
    {
        // Arrange
        var command = new UpdateInvoiceCommand(
            1,
            10,
            DateTime.UtcNow,
            "Notes",
            new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 },
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 0, 2m, 150m)
            });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName.Contains("Items"));
    }

    [Fact]
    public async Task Validate_Should_Fail_When_Item_Quantity_Is_Zero()
    {
        // Arrange
        var command = new UpdateInvoiceCommand(
            1,
            10,
            DateTime.UtcNow,
            "Notes",
            new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 },
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 0m, 150m)
            });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName.Contains("Items"));
    }

    [Fact]
    public async Task Validate_Should_Fail_When_Item_UnitPrice_Is_Zero()
    {
        // Arrange
        var command = new UpdateInvoiceCommand(
            1,
            10,
            DateTime.UtcNow,
            "Notes",
            new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 },
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 2m, 0m)
            });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName.Contains("Items"));
    }
}
