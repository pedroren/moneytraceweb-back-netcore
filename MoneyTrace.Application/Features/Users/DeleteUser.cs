using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Common;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

namespace MoneyTrace.Application.Features.Users;

public record DeleteUserCommand(int UserId) : IRequest<ErrorOr<UserEntity>>;
public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ErrorOr<UserEntity>>
{
    private readonly AppDbContext _context;

    public DeleteUserCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<UserEntity>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            return Error.NotFound("User not found.");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);

        return user;
    }
}

internal sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    private readonly AppDbContext _context;

    public DeleteUserCommandValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0.");
    }
}