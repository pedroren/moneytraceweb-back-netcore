namespace MoneyTrace.Application.Features.Accounts;

using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

public record UpdateAccountCommand(int UserId, int Id, string Name, string Description, decimal Balance, AccountType Type, bool IsEnabled) : IRequest<ErrorOr<AccountEntity>>;
public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, ErrorOr<AccountEntity>>
{
    private readonly AppDbContext _context;

    public UpdateAccountCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<AccountEntity>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
          .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId, cancellationToken);

        if (account == null)
        {
            return Error.NotFound("Account not found.");
        }

        account.Name = request.Name;
        account.Description = request.Description;
        account.Balance = request.Balance;
        account.Type = request.Type;
        account.IsEnabled = request.IsEnabled;

        await _context.SaveChangesAsync(cancellationToken);

        return account;
    }
}
internal sealed class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
  private readonly AppDbContext _context;

  public UpdateAccountCommandValidator(AppDbContext context)
  {
    _context = context;

    RuleFor(x => x.Name)
      .NotEmpty().WithMessage("Name is required.")
      .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
      .MustAsync((m, name, canToken) => BeUniqueName(m.UserId, name, CancellationToken.None))
      .WithMessage("An account with the same name already exists."); ;
    RuleFor(x => x.Description)
      .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    RuleFor(x => x.UserId)
      .GreaterThan(0).WithMessage("User not identified.");
  }

  /// <summary>
  /// Must be unique name for the user
  /// </summary>
  private Task<bool> BeUniqueName(int userId, string name, CancellationToken cancellationToken)
  {
    return _context.Accounts
        .AllAsync(l => l.UserId == userId && l.Name != name, cancellationToken);
  }
}

public record UpdateAccountBalanceCommand(int UserId, int Id, decimal Balance) : IRequest<ErrorOr<AccountEntity>>;
public class UpdateAccountBalanceCommandHandler : IRequestHandler<UpdateAccountBalanceCommand, ErrorOr<AccountEntity>>
{
    private readonly AppDbContext _context;

    public UpdateAccountBalanceCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<AccountEntity>> Handle(UpdateAccountBalanceCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
          .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId, cancellationToken);

        if (account == null)
        {
            return Error.NotFound("Account not found.");
        }
          
        account.Balance += request.Balance;

        await _context.SaveChangesAsync(cancellationToken);        

        return account;
    }
}