namespace FinancialManagementApi.Infrastructure.Sql;

public static class InvoiceSql
{
    public const string InsertInvoiceItem = """
        INSERT INTO InvoiceItems
        (
            InvoiceId,
            ProductId,
            Quantity,
            UnitPrice,
            LineTotal
        )
        VALUES
        (
            @InvoiceId,
            @ProductId,
            @Quantity,
            @UnitPrice,
            @LineTotal
        );
        """;

    public const string GetById = """
        SELECT
            Id,
            InvoiceNumber,
            CustomerId,
            InvoiceDate,
            Status,
            TotalAmount,
            PaidAmount,
            Notes,
            CreatedAt
        FROM Invoices
        WHERE Id = @Id;
        """;

    public const string HasItems = """
        SELECT CAST(CASE WHEN EXISTS
        (
            SELECT 1
            FROM InvoiceItems
            WHERE InvoiceId = @InvoiceId
        )
        THEN 1 ELSE 0 END AS BIT);
        """;

    public const string GetInvoiceTotal = """
        SELECT ISNULL(SUM(LineTotal), 0)
        FROM InvoiceItems
        WHERE InvoiceId = @InvoiceId;
        """;

    public const string GetDetailsHeader = """
        SELECT
            i.Id,
            i.InvoiceNumber,
            i.CustomerId,
            c.Name AS CustomerName,
            i.InvoiceDate,
            i.Status,
            i.TotalAmount,
            i.PaidAmount,
            (i.TotalAmount - i.PaidAmount) AS RemainingAmount,
            i.Notes
        FROM Invoices i
        INNER JOIN Customers c ON c.Id = i.CustomerId
        WHERE i.Id = @Id;
        """;

    public const string GetDetailsItems = """
        SELECT
            ii.ProductId,
            p.Name AS ProductName,
            ii.Quantity,
            ii.UnitPrice,
            ii.LineTotal
        FROM InvoiceItems ii
        INNER JOIN Products p ON p.Id = ii.ProductId
        WHERE ii.InvoiceId = @InvoiceId
        ORDER BY ii.Id;
        """;

    public const string HasCustomerInvoices = """
        SELECT CAST(CASE WHEN EXISTS
        (
            SELECT 1
            FROM Invoices
            WHERE CustomerId = @CustomerId
        )
        THEN 1 ELSE 0 END AS BIT);
        """;

    public const string GetAllPaged = """
        SELECT
            i.Id,
            i.InvoiceNumber,
            i.CustomerId,
            c.Name AS CustomerName,
            i.InvoiceDate,
            i.Status,
            i.TotalAmount,
            i.PaidAmount,
            (i.TotalAmount - i.PaidAmount) AS RemainingAmount,
            i.Notes,
            i.CreatedAt
        FROM Invoices i
        INNER JOIN Customers c ON c.Id = i.CustomerId
        WHERE 1 = 1
            AND (@CustomerId IS NULL OR i.CustomerId = @CustomerId)
            AND (@IncludeIssued = 1 OR i.Status != 0)
            AND (@DateFrom IS NULL OR i.InvoiceDate >= @DateFrom)
            AND (@DateTo IS NULL OR i.InvoiceDate <= @DateTo)
        ORDER BY i.InvoiceDate DESC, i.Id DESC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
        """;

    public const string GetTotalCount = """
        SELECT COUNT(*)
        FROM Invoices i
        WHERE 1 = 1
            AND (@CustomerId IS NULL OR i.CustomerId = @CustomerId)
            AND (@IncludeIssued = 1 OR i.Status != 0)
            AND (@DateFrom IS NULL OR i.InvoiceDate >= @DateFrom)
            AND (@DateTo IS NULL OR i.InvoiceDate <= @DateTo);
        """;
}