/*- Title
- Type: Debit, Credit
- Balance
- Enabled*/

using MoneyTrace.Application.Common;

namespace MoneyTrace.Application.Domain
{
  /// <summary>
  /// Accounts, aka Wallets, are used to track the balance of a user.
  /// They can be of type Debit or Credit.
  /// A Debit account is used to track Cash, Savings and Checking accounts.
  /// A Credit account is used to track Credit cards, Loans and Mortgages.
  /// </summary>
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