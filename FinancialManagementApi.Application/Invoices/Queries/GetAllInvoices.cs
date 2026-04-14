namespace FinancialManagementApi.Application.Invoices.Queries;

public sealed record GetAllInvoicesPagedQuery(
    int? CustomerId,
    bool IncludeIssued,
    DateTime? DateFrom,
    DateTime? DateTo,
    int PageNumber,
    int PageSize);
