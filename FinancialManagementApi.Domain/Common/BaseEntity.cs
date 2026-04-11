using System.ComponentModel.DataAnnotations;

namespace FinancialManagementApi.Domain.Common;

public class BaseEntity
{
    [Key]
    public int Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
}
