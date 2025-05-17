using MoneyTrace.Application.Common;

namespace MoneyTrace.Application.Domain
{
  public class UserEntity : AuditableEntity, IHasDomainEvent
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? PasswordSalt { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string TimeZone { get; set; } = "UTC";
    
    public List<DomainEvent> DomainEvents { get; } = new List<DomainEvent>();
  }
}