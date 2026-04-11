using FinancialManagementApi.Domain.Entities;
using FinancialManagementApi.Domain.Enums;
using FinancialManagementApi.Domain.Exceptions;
using FluentAssertions;

namespace FinancialManagementApi.Tests.Domain.Invoices;

public sealed class InvoiceTests
{
    [Fact]
    public void Constructor_Should_Create_Invoice_With_Expected_Values()
    {
        var invoice = new Invoice(
            "INV-0001",
            1,
            new DateTime(2026, 4, 9),
            "First invoice");

        invoice.InvoiceNumber.Should().Be("INV-0001");
        invoice.CustomerId.Should().Be(1);
        invoice.InvoiceDate.Should().Be(new DateTime(2026, 4, 9));
        invoice.Notes.Should().Be("First invoice");
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.TotalAmount.Should().Be(0m);
        invoice.PaidAmount.Should().Be(0m);
    }

    [Fact]
    public void SetTotalAmount_Should_Update_TotalAmount()
    {
        var invoice = new Invoice(
            "INV-0001",
            1,
            new DateTime(2026, 4, 9),
            null);

        invoice.SetTotalAmount(345m);

        invoice.TotalAmount.Should().Be(345m);
    }

    [Fact]
    public void Issue_Should_Set_Status_To_Issued_When_Invoice_Is_Draft_And_Has_Items()
    {
        var invoice = new Invoice(
            "INV-0001",
            1,
            new DateTime(2026, 4, 9),
            null);

        invoice.Issue(true);

        invoice.Status.Should().Be(InvoiceStatus.Issued);
    }

    [Fact]
    public void Issue_Should_Throw_When_Invoice_Has_No_Items()
    {
        var invoice = new Invoice(
            "INV-0001",
            1,
            new DateTime(2026, 4, 9),
            null);

        var act = () => invoice.Issue(false);

        act.Should().Throw<InvoiceCannotBeIssuedException>()
            .WithMessage("Invoice must contain at least one item before it can be issued.");
    }

    [Fact]
    public void Issue_Should_Throw_When_Invoice_Is_Not_Draft()
    {
        var invoice = new Invoice(
            "INV-0001",
            1,
            new DateTime(2026, 4, 9),
            null);

        invoice.Issue(true);

        var act = () => invoice.Issue(true);

        act.Should().Throw<InvalidInvoiceStateException>()
            .WithMessage("Only draft invoices can be issued.");
    }

    [Fact]
    public void RecordPayment_Should_Throw_When_Amount_Is_Zero()
    {
        var invoice = new Invoice(
            "INV-0001",
            1,
            new DateTime(2026, 4, 9),
            null);

        invoice.SetTotalAmount(100m);
        invoice.Issue(true);

        var act = () => invoice.RecordPayment(0m);

        act.Should().Throw<PaymentAmountMustBeGreaterThanZeroException>();
    }

    [Fact]
    public void RecordPayment_Should_Throw_When_Amount_Exceeds_TotalAmount()
    {
        var invoice = new Invoice(
            "INV-0001",
            1,
            new DateTime(2026, 4, 9),
            null);

        invoice.SetTotalAmount(100m);
        invoice.Issue(true);

        var act = () => invoice.RecordPayment(150m);

        act.Should().Throw<PaymentExceedsInvoiceBalanceException>();
    }

    [Fact]
    public void RecordPayment_Should_Set_Status_To_PartiallyPaid_When_Payment_Is_Less_Than_Total()
    {
        var invoice = new Invoice(
            "INV-0001",
            1,
            new DateTime(2026, 4, 9),
            null);

        invoice.SetTotalAmount(300m);
        invoice.Issue(true);

        invoice.RecordPayment(100m);

        invoice.PaidAmount.Should().Be(100m);
        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
    }

    [Fact]
    public void RecordPayment_Should_Set_Status_To_Paid_When_Payment_Equals_Total()
    {
        var invoice = new Invoice(
            "INV-0001",
            1,
            new DateTime(2026, 4, 9),
            null);

        invoice.SetTotalAmount(300m);
        invoice.Issue(true);

        invoice.RecordPayment(300m);

        invoice.PaidAmount.Should().Be(300m);
        invoice.Status.Should().Be(InvoiceStatus.Paid);
    }

    [Fact]
    public void RecordPayment_Should_Accumulate_PaidAmount_And_Update_Status()
    {
        var invoice = new Invoice(
            "INV-0001",
            1,
            new DateTime(2026, 4, 9),
            null);

        invoice.SetTotalAmount(300m);
        invoice.Issue(true);

        invoice.RecordPayment(100m);
        invoice.RecordPayment(200m);

        invoice.PaidAmount.Should().Be(300m);
        invoice.Status.Should().Be(InvoiceStatus.Paid);
    }
}