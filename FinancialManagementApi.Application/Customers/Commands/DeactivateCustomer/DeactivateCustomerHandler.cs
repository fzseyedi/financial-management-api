using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FluentValidation;

namespace FinancialManagementApi.Application.Customers.Commands.DeactivateCustomer;

public sealed class DeactivateCustomerHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IValidator<DeactivateCustomerCommand> _validator;

    public DeactivateCustomerHandler(
        ICustomerRepository customerRepository,
        IValidator<DeactivateCustomerCommand> validator)
    {
        _customerRepository = customerRepository;
        _validator = validator;
    }

    public async Task HandleAsync(DeactivateCustomerCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var customer = await _customerRepository.GetByIdAsync(command.Id, cancellationToken);
        if (customer is null)
            throw new NotFoundException($"Customer with id {command.Id} was not found.");

        if (!customer.IsActive)
            return;

        customer.Deactivate();

        var updated = await _customerRepository.UpdateAsync(customer, cancellationToken);
        if (!updated)
            throw new BadRequestException("Customer deactivation failed.");
    }
}