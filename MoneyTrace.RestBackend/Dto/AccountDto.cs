using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Accounts;

namespace MoneyTrace.RestBackend.Dto;

public record AccountDto(int Id, string Name, string Description, decimal Balance, string Type, bool IsEnabled);

public static class AccountDtoExtensions
{
    public static UpdateAccountCommand ToUpdateCommand(this AccountDto account, int userId)
    {
        return new UpdateAccountCommand(userId, account.Id, account.Name, account.Description, account.Balance, Enum.Parse<AccountType>(account.Type), account.IsEnabled);
    }

    public static AccountDto ToDto(this AccountEntity account)
    {
        return new AccountDto(account.Id, account.Name, account.Description, account.Balance, account.Type.ToString(), account.IsEnabled);
    }
    public static AccountEntity ToEntity(this AccountDto accountDto)
    {
        return new AccountEntity
        {
            Id = accountDto.Id,
            Name = accountDto.Name,
            Description = accountDto.Description,
            Balance = accountDto.Balance,
            Type = Enum.Parse<AccountType>(accountDto.Type),
            IsEnabled = accountDto.IsEnabled
        };
    }
    public static AccountEntity ToEntity(this AccountDto accountDto, int userId)
    {
        return new AccountEntity
        {
            Id = accountDto.Id,
            Name = accountDto.Name,
            Description = accountDto.Description,
            Balance = accountDto.Balance,
            Type = Enum.Parse<AccountType>(accountDto.Type),
            IsEnabled = accountDto.IsEnabled,
            UserId = userId
        };
    }
    public static void UpdateFromDto(this AccountEntity account, AccountDto accountDto)
    {
        account.Name = accountDto.Name;
        account.Description = accountDto.Description;
        account.Balance = accountDto.Balance;
        account.Type = Enum.Parse<AccountType>(accountDto.Type);
        account.IsEnabled = accountDto.IsEnabled;
    }
}
