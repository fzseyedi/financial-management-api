using FinancialManagementApi.Domain.Entities;
using FluentAssertions;

namespace FinancialManagementApi.Tests.Domain.Invoices;

public sealed class InvoiceItemTests
{
    [Fact]
    public void Constructor_Should_Create_InvoiceItem_With_Expected_Values()
    {
        var item = new InvoiceItem(
            2,
            3m,
            150m);

        item.ProductId.Should().Be(2);
        item.Quantity.Should().Be(3m);
        item.UnitPrice.Should().Be(150m);
        item.LineTotal.Should().Be(450m);
        item.InvoiceId.Should().Be(0);
    }

    [Fact]
    public void AssignInvoice_Should_Set_InvoiceId()
    {
        var item = new InvoiceItem(
            2,
            3m,
            150m);

        item.AssignInvoice(10);

        item.InvoiceId.Should().Be(10);
    }

    [Fact]
    public void Constructor_Should_Calculate_LineTotal_Correctly()
    {
        var item = new InvoiceItem(
            20,
            2.5m,
            99.99m);

        item.LineTotal.Should().Be(249.975m);
    }
}