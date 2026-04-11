using FinancialManagementApi.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialManagementApi.Domain.Entities;

[Table("Payments")]
public class Payment : BaseEntity
{
    public int CustomerId { get; private set; }
    public int InvoiceId { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public decimal Amount { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? Notes { get; private set; }

    private Payment()
    {
    }

    public Payment(
        int customerId,
        int invoiceId,
        DateTime paymentDate,
        decimal amount,
        string? referenceNumber,
        string? notes)
    {
        CustomerId = customerId;
        InvoiceId = invoiceId;
        PaymentDate = paymentDate;
        Amount = amount;
        ReferenceNumber = referenceNumber;
        Notes = notes;
    }
}