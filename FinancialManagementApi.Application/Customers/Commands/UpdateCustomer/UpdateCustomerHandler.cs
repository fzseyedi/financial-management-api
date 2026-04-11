using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FluentValidation;

namespace FinancialManagementApi.Application.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IValidator<UpdateCustomerCommand> _validator;

    public UpdateCustomerHandler(
        ICustomerRepository customerRepository,
        IValidator<UpdateCustomerCommand> validator)
    {
        _customerRepository = customerRepository;
        _validator = validator;
    }

    public async Task HandleAsync(UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var customer = await _customerRepository.GetByIdAsync(command.Id, cancellationToken);
        if (customer is null)
            throw new NotFoundException($"Customer with id {command.Id} was not found.");

        var exists = await _customerRepository.ExistsByCodeAsync(command.Code, command.Id, cancellationToken);
        if (exists)
            throw new ConflictException($"Customer code '{command.Code}' already exists.");

        customer.Update(
            command.Code.Trim(),
            command.Name.Trim(),
            string.IsNullOrWhiteSpace(command.Email) ? null : command.Email.Trim(),
            string.IsNullOrWhiteSpace(command.Phone) ? null : command.Phone.Trim(),
            string.IsNullOrWhiteSpace(command.Address) ? null : command.Address.Trim());

        var updated = await _customerRepository.UpdateAsync(customer, cancellationToken);
        if (!updated)
            throw new BadRequestException("Customer update failed.");
    }
}