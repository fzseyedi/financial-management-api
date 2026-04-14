namespace FinancialManagementApi.Infrastructure.Sql;

public static class CustomerSql
{
    public const string GetById = """
        SELECT
            Id,
            Code,
            Name,
            Email,
            Phone,
            Address,
            IsActive,
            CreatedAt
        FROM Customers
        WHERE Id = @Id;
        """;

    public const string ExistsByCode = """
        SELECT CAST(CASE WHEN EXISTS
        (
            SELECT 1
            FROM Customers
            WHERE Code = @Code
        )
        THEN 1 ELSE 0 END AS BIT);
        """;

    public const string ExistsByCodeExcludingId = """
        SELECT CAST(CASE WHEN EXISTS
        (
            SELECT 1
            FROM Customers
            WHERE Code = @Code
              AND Id <> @ExcludeId
        )
        THEN 1 ELSE 0 END AS BIT);
        """;

    public const string GetAllActive = """
        SELECT
            Id,
            Code,
            Name,
            Email,
            Phone,
            Address,
            IsActive,
            CreatedAt
        FROM Customers
        WHERE IsActive = 1
        ORDER BY Name;
        """;

    public const string GetAllIncludingInactive = """
        SELECT
            Id,
            Code,
            Name,
            Email,
            Phone,
            Address,
            IsActive,
            CreatedAt
        FROM Customers
        ORDER BY Name;
        """;

    public const string GetAllActivePaged = """
        SELECT
            Id,
            Code,
            Name,
            Email,
            Phone,
            Address,
            IsActive,
            CreatedAt
        FROM Customers
        WHERE IsActive = 1
        ORDER BY Name
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
        """;

    public const string GetAllIncludingInactivePaged = """
        SELECT
            Id,
            Code,
            Name,
            Email,
            Phone,
            Address,
            IsActive,
            CreatedAt
        FROM Customers
        ORDER BY Name
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
        """;

    public const string GetTotalCountActive = """
        SELECT COUNT(*)
        FROM Customers
        WHERE IsActive = 1;
        """;

    public const string GetTotalCountIncludingInactive = """
        SELECT COUNT(*)
        FROM Customers;
        """;

    public const string Delete = """
        DELETE FROM Customers
        WHERE Id = @Id;
        """;
}
