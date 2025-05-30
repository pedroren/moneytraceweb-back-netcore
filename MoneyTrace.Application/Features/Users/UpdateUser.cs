using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Common;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

namespace MoneyTrace.Application.Features.Users;

public record UpdateUserCommand(int UserId, string Name, string Email, string Password) : IRequest<ErrorOr<UserEntity>>;
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ErrorOr<UserEntity>>
{
    private readonly AppDbContext _context;

    public UpdateUserCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<UserEntity>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            return Error.NotFound("User not found.");
        }

        user.Name = request.Name;
        user.Email = request.Email;
        user.PasswordHash = request.Password; //todo: hash password
        user.PasswordSalt = request.Password; //todo: generate salt
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        return user;
    }
}

internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    private readonly AppDbContext _context;

    public UpdateUserCommandValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("A valid email is required.")
            .MustAsync(BeUniqueEmail)
            .WithMessage("Email must be unique.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return !await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }
}