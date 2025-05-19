using MoneyTrace.Application.Common;

namespace MoneyTrace.Application.Domain;

public class VendorEntity : AuditableEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int UserId { get; set; }
}