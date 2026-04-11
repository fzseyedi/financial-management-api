using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.DTOs;

namespace FinancialManagementApi.Application.Reports.Queries.GetUnpaidInvoices;

public sealed class GetUnpaidInvoicesHandler
{
    private readonly IPaymentRepository _paymentRepository;

    public GetUnpaidInvoicesHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<IEnumerable<UnpaidInvoiceDto>> HandleAsync(GetUnpaidInvoicesQuery query, CancellationToken cancellationToken)
    {
        return await _paymentRepository.GetUnpaidInvoicesAsync(cancellationToken);
    }
}