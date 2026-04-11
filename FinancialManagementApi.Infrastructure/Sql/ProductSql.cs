namespace FinancialManagementApi.Infrastructure.Sql;

public static class ProductSql
{
    public const string GetById = """
        SELECT
            Id,
            Code,
            Name,
            UnitPrice,
            IsActive,
            CreatedAt
        FROM Products
        WHERE Id = @Id;
        """;

    public const string ExistsByCode = """
        SELECT CAST(CASE WHEN EXISTS
        (
            SELECT 1
            FROM Products
            WHERE Code = @Code
        )
        THEN 1 ELSE 0 END AS BIT);
        """;

    public const string ExistsByCodeExcludingId = """
        SELECT CAST(CASE WHEN EXISTS
        (
            SELECT 1
            FROM Products
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
            UnitPrice,
            IsActive,
            CreatedAt
        FROM Products
        WHERE IsActive = 1
        ORDER BY Name;
        """;

    public const string GetAllIncludingInactive = """
        SELECT
            Id,
            Code,
            Name,
            UnitPrice,
            IsActive,
            CreatedAt
        FROM Products
        ORDER BY Name;
        """;
}