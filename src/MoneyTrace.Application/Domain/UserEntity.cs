using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using MoneyTrace.Application.Common;

namespace MoneyTrace.Application.Domain;

  public class UserEntity : AuditableEntity, IHasDomainEvent
  {
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? PasswordSalt { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string TimeZone { get; set; } = "UTC";
    public bool AutoCreateBudget { get; set; } = true; // Auto-create next budget on user creation or new period start
    
    [NotMapped]
    public List<DomainEvent> DomainEvents { get; private set; } = [];
  }
