using FinancialManagementApi.Application.DTOs;
using FinancialManagementApi.Domain.Entities;

namespace FinancialManagementApi.Application.Customers;

public static class CustomerMappings
{
    public static CustomerDto ToDto(this Customer customer)
    {
        return new CustomerDto(
            customer.Id,
            customer.Code,
            customer.Name,
            customer.Email,
            customer.Phone,
            customer.Address,
            customer.IsActive);
    }
}