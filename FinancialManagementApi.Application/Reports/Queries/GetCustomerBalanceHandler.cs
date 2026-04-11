using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Application.DTOs;

namespace FinancialManagementApi.Application.Reports.Queries;

public sealed class GetCustomerBalanceHandler
{
    private readonly IPaymentRepository _paymentRepository;

    public GetCustomerBalanceHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<CustomerBalanceDto> HandleAsync(GetCustomerBalanceQuery query, CancellationToken cancellationToken)
    {
        var result = await _paymentRepository.GetCustomerBalanceAsync(query.CustomerId, cancellationToken);

        if (result is null)
            throw new NotFoundException($"Customer with id {query.CustomerId} was not found.");

        return result;
    }
}