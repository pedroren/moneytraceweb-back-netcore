using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

namespace MoneyTrace.Application.Features.Accounts
{
  public record CreateAccountCommand(int UserId, string Name, string Description, decimal Balance, AccountType Type) : IRequest<ErrorOr<AccountEntity>>;
  public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, ErrorOr<AccountEntity>>
  {
    private readonly IAppDbContext _context;

    public CreateAccountCommandHandler(IAppDbContext context)
    {
      _context = context;
    }

    public async Task<ErrorOr<AccountEntity>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
      var account = new AccountEntity
      {
        Name = request.Name,
        Description = request.Description,
        UserId = request.UserId,
        Balance = request.Balance,
        Type = request.Type,
        IsEnabled = true
      };

      _context.Accounts.Add(account);
      await _context.SaveChangesAsync(cancellationToken);

      return account;
    }
  }
  public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
  {
    private readonly IAppDbContext _context;
    public CreateAccountCommandValidator(IAppDbContext context)
    {
      _context = context;
      
      RuleFor(x => x.Name)
        .NotEmpty().WithMessage("Name is required.")
        .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
        .MustAsync((m, name, canToken) => BeUniqueName(m.UserId, name, CancellationToken.None))
        .WithMessage("An account with the same name already exists.");;
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
}