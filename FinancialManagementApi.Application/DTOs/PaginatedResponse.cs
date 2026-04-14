namespace FinancialManagementApi.Application.DTOs;

/// <summary>
/// Generic paginated response containing items and pagination metadata.
/// </summary>
/// <typeparam name="T">The type of items in the paginated response</typeparam>
public sealed record PaginatedResponse<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);
