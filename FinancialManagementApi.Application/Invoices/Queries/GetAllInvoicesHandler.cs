using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.DTOs;

namespace FinancialManagementApi.Application.Invoices.Queries;

public sealed class GetAllInvoicesHandler
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetAllInvoicesHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<PaginatedResponse<InvoiceSummaryDto>> HandlePagedAsync(
        GetAllInvoicesPagedQuery query,
        CancellationToken cancellationToken)
    {
        var (invoices, totalCount) = await _invoiceRepository.GetAllPagedAsync(
            query.CustomerId,
            query.IncludeIssued,
            query.DateFrom,
            query.DateTo,
            query.PageNumber,
            query.PageSize,
            cancellationToken);

        var totalPages = (totalCount + query.PageSize - 1) / query.PageSize;

        return new PaginatedResponse<InvoiceSummaryDto>(
            invoices,
            totalCount,
            query.PageNumber,
            query.PageSize,
            totalPages);
    }
}
