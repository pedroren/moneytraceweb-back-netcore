namespace MoneyTrace.Application.Features.Accounts;

using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

public record GetUserAccountsQuery(int UserId) : IRequest<ErrorOr<List<AccountEntity>>>;
public class GetUserAccountsQueryHandler : IRequestHandler<GetUserAccountsQuery, ErrorOr<List<AccountEntity>>>
{
    private readonly AppDbContext _context;

    public GetUserAccountsQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<AccountEntity>>> Handle(GetUserAccountsQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId <= 0)
        {
            return Error.Validation("User not identified.");
        }
        return await _context.Accounts
          .Where(x => x.UserId == request.UserId)
          .ToListAsync(cancellationToken);
    }
}
public record GetAccountByIdQuery(int UserId, int AccountId) : IRequest<ErrorOr<AccountEntity>>;
public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, ErrorOr<AccountEntity>>
{
    private readonly AppDbContext _context;

    public GetAccountByIdQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<AccountEntity>> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts.AsNoTracking()
          .FirstOrDefaultAsync(x => x.Id == request.AccountId && x.UserId == request.UserId, cancellationToken);

        if (account == null)
        {
            return Error.NotFound("Account not found.");
        }

        return account;
    }
}