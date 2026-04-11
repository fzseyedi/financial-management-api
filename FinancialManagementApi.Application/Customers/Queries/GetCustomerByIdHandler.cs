using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.DTOs;

namespace FinancialManagementApi.Application.Customers.Queries;

public sealed class GetCustomerByIdHandler
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByIdHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDto?> HandleAsync(GetCustomerByIdQuery query, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(query.Id, cancellationToken);
        return customer?.ToDto();
    }
}