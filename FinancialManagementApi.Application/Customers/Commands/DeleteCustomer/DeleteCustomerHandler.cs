using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;

namespace FinancialManagementApi.Application.Customers.Commands.DeleteCustomer;

public sealed class DeleteCustomerHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;

    public DeleteCustomerHandler(
        ICustomerRepository customerRepository,
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository)
    {
        _customerRepository = customerRepository;
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task HandleAsync(DeleteCustomerCommand command, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(command.Id, cancellationToken);
        if (customer is null)
            throw new NotFoundException($"Customer with id {command.Id} was not found.");

        var hasInvoices = await _invoiceRepository.HasCustomerInvoicesAsync(command.Id, cancellationToken);
        if (hasInvoices)
            throw new ConflictException("Customer cannot be deleted because they have associated invoices.");

        var hasPayments = await _paymentRepository.HasCustomerPaymentsAsync(command.Id, cancellationToken);
        if (hasPayments)
            throw new ConflictException("Customer cannot be deleted because they have associated payments.");

        var deleted = await _customerRepository.DeleteAsync(command.Id, cancellationToken);
        if (!deleted)
            throw new BadRequestException("Customer deletion failed.");
    }
}
