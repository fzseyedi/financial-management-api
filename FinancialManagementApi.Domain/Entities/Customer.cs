using FinancialManagementApi.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialManagementApi.Domain.Entities;

[Table("Customers")]
public class Customer : BaseEntity
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Customer()
    {
    }

    public Customer(string code, string name, string? email, string? phone, string? address)
    {
        Code = code;
        Name = name;
        Email = email;
        Phone = phone;
        Address = address;
    }

    public void Update(string code, string name, string? email, string? phone, string? address)
    {
        Code = code;
        Name = name;
        Email = email;
        Phone = phone;
        Address = address;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}