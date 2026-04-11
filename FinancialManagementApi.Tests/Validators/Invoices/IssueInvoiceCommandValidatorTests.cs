using FinancialManagementApi.Application.Invoices.Commands;
using FluentAssertions;

namespace FinancialManagementApi.Tests.Validators.Invoices;

public sealed class IssueInvoiceCommandValidatorTests
{
    private readonly IssueInvoiceCommandValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_Should_Pass_When_InvoiceId_Is_Valid()
    {
        var command = new IssueInvoiceCommand(1);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_InvoiceId_Is_Zero()
    {
        var command = new IssueInvoiceCommand(0);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "InvoiceId");
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_When_InvoiceId_Is_Negative()
    {
        var command = new IssueInvoiceCommand(-1);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "InvoiceId");
    }
}