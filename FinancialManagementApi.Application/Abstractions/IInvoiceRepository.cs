using FinancialManagementApi.Application.DTOs;
using FinancialManagementApi.Domain.Entities;

namespace FinancialManagementApi.Application.Abstractions;

public interface IInvoiceRepository
{
    Task<int> CreateWithItemsAsync(
        Invoice invoice,
        IReadOnlyCollection<InvoiceItem> items,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(Invoice invoice, CancellationToken cancellationToken);

    Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<InvoiceDto?> GetDetailsByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> HasItemsAsync(int invoiceId, CancellationToken cancellationToken);
    Task<decimal> GetInvoiceTotalAsync(int invoiceId, CancellationToken cancellationToken);
}