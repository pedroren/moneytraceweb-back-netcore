namespace MoneyTrace.Application.Features.Accounts;

using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

public record DeleteAccountCommand(int UserId, int Id) : IRequest<ErrorOr<AccountEntity>>;
public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, ErrorOr<AccountEntity>>
{
    private readonly AppDbContext _context;

    public DeleteAccountCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<AccountEntity>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
          .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId, cancellationToken);

        if (account == null)
        {
            return Error.NotFound("Account not found.");
        }

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync(cancellationToken);

        return account;
    }
}

internal sealed class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    private readonly AppDbContext _context;

    public DeleteAccountCommandValidator(AppDbContext context)
    {
        _context = context;
        RuleFor(x => x.UserId)
          .GreaterThan(0).WithMessage("User not identified.");
          RuleFor(x => x.Id)
          .GreaterThan(0).WithMessage("Account not identified.");
        //TODO: Validate if the account is not used in any transaction
    }
}

