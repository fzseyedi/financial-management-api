using FinancialManagementApi.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialManagementApi.Domain.Entities;

[Table("InvoiceItems")]
public class InvoiceItem : BaseEntity
{
    public int InvoiceId { get; private set; }
    public int ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }

    private InvoiceItem()
    {
    }

    public InvoiceItem(int productId, decimal quantity, decimal unitPrice)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = quantity * unitPrice;
    }

    public void AssignInvoice(int invoiceId)
    {
        InvoiceId = invoiceId;
    }
}