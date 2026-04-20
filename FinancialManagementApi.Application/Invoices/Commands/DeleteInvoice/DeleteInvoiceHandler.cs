using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Domain.Enums;

namespace FinancialManagementApi.Application.Invoices.Commands.DeleteInvoice;

public sealed class DeleteInvoiceHandler
{
    private readonly IInvoiceRepository _invoiceRepository;

    public DeleteInvoiceHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task HandleAsync(DeleteInvoiceCommand command, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(command.Id, cancellationToken);
        if (invoice is null)
            throw new NotFoundException($"Invoice with id {command.Id} was not found.");

        if (invoice.Status != InvoiceStatus.Draft)
            throw new ConflictException("Only draft invoices can be deleted.");

        var deleted = await _invoiceRepository.DeleteAsync(command.Id, cancellationToken);
        if (!deleted)
            throw new BadRequestException("Invoice deletion failed.");
    }
}
