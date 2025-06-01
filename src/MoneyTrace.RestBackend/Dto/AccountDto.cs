using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Accounts;

namespace MoneyTrace.RestBackend.Dto;

/// <summary>
/// Dto for AccountEntity.
/// </summary>
public record AccountDto(int Id, string Name, string Description, decimal Balance, AccountType Type, bool IsEnabled);


public static class AccountDtoExtensions
{
    public static UpdateAccountCommand ToUpdateCommand(this AccountDto dto, int userId)
    {
        return new UpdateAccountCommand(userId, dto.Id, dto.Name.Trim(), dto.Description?.Trim(), dto.Balance, dto.Type, dto.IsEnabled);
    }

    public static CreateAccountCommand ToCreateCommand(this AccountDto dto, int userId)
    {
        return new CreateAccountCommand(userId, dto.Name.Trim(), dto.Description?.Trim(), dto.Balance, dto.Type);
    }

    public static AccountDto ToDto(this AccountEntity account)
    {
        return new AccountDto(account.Id, account.Name, account.Description, account.Balance, account.Type, account.IsEnabled);
    }
    public static AccountEntity ToEntity(this AccountDto accountDto)
    {
        return new AccountEntity
        {
            Id = accountDto.Id,
            Name = accountDto.Name,
            Description = accountDto.Description,
            Balance = accountDto.Balance,
            Type = accountDto.Type,
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
            Type = accountDto.Type,
            IsEnabled = accountDto.IsEnabled,
            UserId = userId
        };
    }
    public static void UpdateFromDto(this AccountEntity account, AccountDto accountDto)
    {
        account.Name = accountDto.Name;
        account.Description = accountDto.Description;
        account.Balance = accountDto.Balance;
        account.Type = accountDto.Type;
        account.IsEnabled = accountDto.IsEnabled;
    }
}
