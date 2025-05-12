/*- Title
- Type: Debit, Credit
- Balance
- Enabled*/

using MoneyTrace.Application.Common;

namespace MoneyTrace.Application.Domain
{
  public class AccountEntity : AuditableEntity
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 0;
    public AccountType Type { get; set; } = AccountType.Credit; // Debit or Credit
    public bool IsEnabled { get; set; } = true;
    public int UserId { get; set; }
  }

  public enum AccountType
  {
    Debit,
    Credit
  }
  
}