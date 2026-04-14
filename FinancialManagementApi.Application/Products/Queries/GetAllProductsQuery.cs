namespace FinancialManagementApi.Application.Products.Queries;

public sealed record GetAllProductsQuery(bool IncludeInactive);

public sealed record GetAllProductsPagedQuery(
    bool IncludeInactive,
    int PageNumber,
    int PageSize);