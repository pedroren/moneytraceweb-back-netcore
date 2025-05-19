using FluentValidation;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Accounts;

namespace MoneyTrace.RestBackend.Dto;

/// <summary>
/// Dto for AccountEntity.
/// </summary>
public record AccountDto(int Id, string Name, string Description, decimal Balance, string Type, bool IsEnabled);

public sealed class AccountDtoValidator : AbstractValidator<AccountDto>
{
    public AccountDtoValidator()
    {        
        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Type is required.")
            .Must(type => Enum.IsDefined(typeof(AccountType), type))
            .WithMessage("Type is not valid.");
    }
}

public static class AccountDtoExtensions
{
    public static UpdateAccountCommand ToUpdateCommand(this AccountDto dto, int userId)
    {
        new AccountDtoValidator().ValidateAndThrow(dto);
        return new UpdateAccountCommand(userId, dto.Id, dto.Name, dto.Description, dto.Balance, Enum.Parse<AccountType>(dto.Type), dto.IsEnabled);
    }

    public static CreateAccountCommand ToCreateCommand(this AccountDto dto, int userId)
    {
        new AccountDtoValidator().ValidateAndThrow(dto);
        return new CreateAccountCommand(userId, dto.Name, dto.Description, dto.Balance, Enum.Parse<AccountType>(dto.Type));
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
