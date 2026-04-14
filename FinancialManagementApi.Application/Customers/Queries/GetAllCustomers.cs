namespace FinancialManagementApi.Application.Customers.Queries;

public sealed record GetAllCustomersQuery(bool IncludeInactive);

public sealed record GetAllCustomersPagedQuery(
    bool IncludeInactive,
    int PageNumber,
    int PageSize);