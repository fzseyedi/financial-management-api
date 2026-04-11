using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.DTOs;

namespace FinancialManagementApi.Application.Customers.Queries;

public sealed class GetAllCustomersHandler
{
    private readonly ICustomerRepository _customerRepository;

    public GetAllCustomersHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<IEnumerable<CustomerDto>> HandleAsync(GetAllCustomersQuery query, CancellationToken cancellationToken)
    {
        var customers = await _customerRepository.GetAllAsync(query.IncludeInactive, cancellationToken);
        return customers.Select(x => x.ToDto());
    }
}