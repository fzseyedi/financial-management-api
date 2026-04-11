namespace FinancialManagementApi.Infrastructure.Sql;

public static class PaymentSql
{
    public const string GetCustomerBalance = """
        SELECT
            c.Id AS CustomerId,
            c.Name AS CustomerName,
            ISNULL(SUM(i.TotalAmount), 0) AS TotalInvoiced,
            ISNULL(SUM(i.PaidAmount), 0) AS TotalPaid,
            ISNULL(SUM(i.TotalAmount - i.PaidAmount), 0) AS Balance
        FROM Customers c
        LEFT JOIN Invoices i ON i.CustomerId = c.Id
        WHERE c.Id = @CustomerId
        GROUP BY c.Id, c.Name;
        """;

    public const string GetUnpaidInvoices = """
        SELECT
            i.Id AS InvoiceId,
            i.InvoiceNumber,
            i.CustomerId,
            c.Name AS CustomerName,
            i.InvoiceDate,
            i.TotalAmount,
            i.PaidAmount,
            (i.TotalAmount - i.PaidAmount) AS RemainingAmount,
            i.Status
        FROM Invoices i
        INNER JOIN Customers c ON c.Id = i.CustomerId
        WHERE i.TotalAmount > i.PaidAmount
        ORDER BY i.InvoiceDate, i.Id;
        """;
}