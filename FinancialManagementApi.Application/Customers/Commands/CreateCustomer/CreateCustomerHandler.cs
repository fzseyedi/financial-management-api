using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Domain.Entities;
using FluentValidation;

namespace FinancialManagementApi.Application.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IValidator<CreateCustomerCommand> _validator;

    public CreateCustomerHandler(
        ICustomerRepository customerRepository,
        IValidator<CreateCustomerCommand> validator)
    {
        _customerRepository = customerRepository;
        _validator = validator;
    }

    public async Task<int> HandleAsync(CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var exists = await _customerRepository.ExistsByCodeAsync(command.Code, cancellationToken);
        if (exists)
            throw new ConflictException($"Customer code '{command.Code}' already exists.");

        var customer = new Customer(
            command.Code.Trim(),
            command.Name.Trim(),
            string.IsNullOrWhiteSpace(command.Email) ? null : command.Email.Trim(),
            string.IsNullOrWhiteSpace(command.Phone) ? null : command.Phone.Trim(),
            string.IsNullOrWhiteSpace(command.Address) ? null : command.Address.Trim());

        return await _customerRepository.CreateAsync(customer, cancellationToken);
    }
}