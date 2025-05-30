using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Common;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

namespace MoneyTrace.Application.Features.Users;

public record GetUsersQuery() : IRequest<ErrorOr<List<UserEntity>>>;
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, ErrorOr<List<UserEntity>>>
{
  private readonly AppDbContext _context;

  public GetUsersQueryHandler(AppDbContext context)
  {
    _context = context;
  }

  public async Task<ErrorOr<List<UserEntity>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
  {
    var users = await _context.Users
        .AsNoTracking()
        .ToListAsync(cancellationToken);

    if (users is null || !users.Any())
    {
      return Error.NotFound("No users found.");
    }

    return users;
  }
}
public record GetUserByIdQuery(int UserId) : IRequest<ErrorOr<UserEntity>>;
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ErrorOr<UserEntity>>
{
  private readonly AppDbContext _context;

  public GetUserByIdQueryHandler(AppDbContext context)
  {
    _context = context;
  }

  public async Task<ErrorOr<UserEntity>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
  {
    var user = await _context.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

    if (user is null)
    {
      return Error.NotFound("User not found.");
    }

    return user;
  }
}