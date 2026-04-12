using System.Data;
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

    /// <summary>
    /// Updates an invoice within an existing database transaction.
    /// Used for atomic operations involving multiple repositories.
    /// </summary>
    Task<bool> UpdateAsync(Invoice invoice, IDbTransaction transaction, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves an invoice by ID with pessimistic locking (row-level lock for update).
    /// Prevents concurrent modifications during payment recording.
    /// </summary>
    Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves an invoice by ID with pessimistic locking within a transaction.
    /// </summary>
    Task<Invoice?> GetByIdWithLockAsync(int id, IDbTransaction transaction, CancellationToken cancellationToken);

    Task<InvoiceDto?> GetDetailsByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> HasItemsAsync(int invoiceId, CancellationToken cancellationToken);
    Task<decimal> GetInvoiceTotalAsync(int invoiceId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if a customer has any invoices.
    /// </summary>
    Task<bool> HasCustomerInvoicesAsync(int customerId, CancellationToken cancellationToken);
}