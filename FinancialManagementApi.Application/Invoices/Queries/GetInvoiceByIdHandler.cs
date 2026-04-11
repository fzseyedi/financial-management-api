using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.DTOs;

namespace FinancialManagementApi.Application.Invoices.Queries;

public sealed class GetInvoiceByIdHandler
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoiceByIdHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<InvoiceDto?> HandleAsync(GetInvoiceByIdQuery query, CancellationToken cancellationToken)
    {
        return await _invoiceRepository.GetDetailsByIdAsync(query.Id, cancellationToken);
    }
}