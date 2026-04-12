using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FluentValidation;

namespace FinancialManagementApi.Application.Payments.Commands.RecordPayment;

public sealed class RecordPaymentHandler
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<RecordPaymentCommand> _validator;

    public RecordPaymentHandler(
        IPaymentRepository paymentRepository,
        ICustomerRepository customerRepository,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        IValidator<RecordPaymentCommand> validator)
    {
        _paymentRepository = paymentRepository;
        _customerRepository = customerRepository;
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<int> HandleAsync(RecordPaymentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var customer = await _customerRepository.GetByIdAsync(command.CustomerId, cancellationToken);
        if (customer is null)
            throw new NotFoundException($"Customer with id {command.CustomerId} was not found.");

        // Execute invoice update and payment creation within a single transaction
        // This ensures atomicity: either both succeed or both fail
        var paymentId = await _unitOfWork.ExecuteInTransactionAsync(
            async transaction =>
            {
                // Use pessimistic locking to prevent concurrent payment modifications
                var invoice = await _invoiceRepository.GetByIdWithLockAsync(command.InvoiceId, transaction, cancellationToken);
                if (invoice is null)
                    throw new NotFoundException($"Invoice with id {command.InvoiceId} was not found.");

                if (invoice.CustomerId != command.CustomerId)
                    throw new BadRequestException("The specified invoice does not belong to the specified customer.");

                invoice.RecordPayment(command.Amount);

                var updated = await _invoiceRepository.UpdateAsync(invoice, transaction, cancellationToken);
                if (!updated)
                    throw new BadRequestException("Invoice payment update failed.");

                var payment = new Domain.Entities.Payment(
                    command.CustomerId,
                    command.InvoiceId,
                    command.PaymentDate,
                    command.Amount,
                    string.IsNullOrWhiteSpace(command.ReferenceNumber) ? null : command.ReferenceNumber.Trim(),
                    string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim());

                return await _paymentRepository.CreateAsync(payment, transaction, cancellationToken);
            },
            cancellationToken);

        return paymentId;
    }
}
