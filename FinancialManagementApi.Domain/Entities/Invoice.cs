using FinancialManagementApi.Domain.Common;
using FinancialManagementApi.Domain.Enums;
using FinancialManagementApi.Domain.Exceptions;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialManagementApi.Domain.Entities;

[Table("Invoices")]
public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; private set; } = default!;
    public int CustomerId { get; private set; }
    public DateTime InvoiceDate { get; private set; }
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public string? Notes { get; private set; }

    private Invoice()
    {
    }

    public Invoice(string invoiceNumber, int customerId, DateTime invoiceDate, string? notes)
    {
        InvoiceNumber = invoiceNumber;
        CustomerId = customerId;
        InvoiceDate = invoiceDate;
        Notes = notes;
        Status = InvoiceStatus.Draft;
        TotalAmount = 0m;
        PaidAmount = 0m;
    }

    public void SetTotalAmount(decimal totalAmount)
    {
        TotalAmount = totalAmount;
    }

    public void Issue(bool hasItems)
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidInvoiceStateException("Only draft invoices can be issued.");

        if (!hasItems)
            throw new InvoiceCannotBeIssuedException();

        Status = InvoiceStatus.Issued;
    }

    public void RecordPayment(decimal amount)
    {
        if (amount <= 0)
            throw new PaymentAmountMustBeGreaterThanZeroException();

        if (PaidAmount + amount > TotalAmount)
            throw new PaymentExceedsInvoiceBalanceException();

        PaidAmount += amount;

        if (PaidAmount == 0)
            Status = InvoiceStatus.Issued;
        else if (PaidAmount < TotalAmount)
            Status = InvoiceStatus.PartiallyPaid;
        else
            Status = InvoiceStatus.Paid;
    }
}