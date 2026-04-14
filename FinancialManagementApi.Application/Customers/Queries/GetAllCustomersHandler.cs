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

    public async Task<PaginatedResponse<CustomerDto>> HandlePagedAsync(GetAllCustomersPagedQuery query, CancellationToken cancellationToken)
    {
        var (customers, totalCount) = await _customerRepository.GetAllPagedAsync(
            query.IncludeInactive,
            query.PageNumber,
            query.PageSize,
            cancellationToken);

        var totalPages = (totalCount + query.PageSize - 1) / query.PageSize;

        return new PaginatedResponse<CustomerDto>(
            customers.Select(x => x.ToDto()),
            totalCount,
            query.PageNumber,
            query.PageSize,
            totalPages);
    }
}