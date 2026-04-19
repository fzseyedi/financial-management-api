using System.ComponentModel.DataAnnotations;

namespace FinancialManagementApi.Domain.Common;

public class BaseEntity
{
    [Key]
    public int Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; protected set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; protected set; }
    [Timestamp]
    public byte[] Version { get; protected set; } = [];
}
