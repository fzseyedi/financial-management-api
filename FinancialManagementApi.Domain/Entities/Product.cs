using FinancialManagementApi.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialManagementApi.Domain.Entities;

[Table("Products")]
public class Product : BaseEntity
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public decimal UnitPrice { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Product()
    {
    }

    public Product(string code, string name, decimal unitPrice)
    {
        Code = code;
        Name = name;
        UnitPrice = unitPrice;
    }

    public void Update(string code, string name, decimal unitPrice)
    {
        Code = code;
        Name = name;
        UnitPrice = unitPrice;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}