using FinancialManagementApi.Application.Payments.Commands.RecordPayment;
using FluentAssertions;

namespace FinancialManagementApi.Tests.Validators.Payments;

public sealed class RecordPaymentCommandValidatorTests
{
    private readonly RecordPaymentCommandValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_Should_Pass_When_Command_Is_Valid()
    {
        var command = new RecordPaymentCommand(
            1,
            2,
            new DateTime(2026, 4, 9),
            100m,
            "PAY-001",
            "First payment");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_CustomerId_Is_Invalid()
    {
        var command = new RecordPaymentCommand(
            0,
            2,
            new DateTime(2026, 4, 9),
            100m,
            "PAY-001",
            null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "CustomerId");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_InvoiceId_Is_Invalid()
    {
        var command = new RecordPaymentCommand(
            1,
            0,
            new DateTime(2026, 4, 9),
            100m,
            "PAY-001",
            null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "InvoiceId");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_PaymentDate_Is_Empty()
    {
        var command = new RecordPaymentCommand(
            1,
            2,
            default,
            100m,
            "PAY-001",
            null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "PaymentDate");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Amount_Is_Zero()
    {
        var command = new RecordPaymentCommand(
            1,
            2,
            new DateTime(2026, 4, 9),
            0m,
            "PAY-001",
            null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Amount");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_ReferenceNumber_Exceeds_MaxLength()
    {
        var command = new RecordPaymentCommand(
            1,
            2,
            new DateTime(2026, 4, 9),
            100m,
            new string('A', 101),
            null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "ReferenceNumber");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_Notes_Exceed_MaxLength()
    {
        var command = new RecordPaymentCommand(
            1,
            2,
            new DateTime(2026, 4, 9),
            100m,
            "PAY-001",
            new string('A', 501));

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Notes");
    }
}