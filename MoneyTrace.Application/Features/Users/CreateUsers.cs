using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Common;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

namespace MoneyTrace.Application.Features.Users
{
  public record CreateUserCommand(string Name, string Email, string Password) : IRequest<ErrorOr<UserEntity>>;
  public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ErrorOr<UserEntity>>
  {
    private readonly IAppDbContext _context;

    public CreateUserCommandHandler(IAppDbContext context)
    {
      _context = context;
    }

    public async Task<ErrorOr<UserEntity>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
      var user = new UserEntity
      {
        Name = request.Name,
        Email = request.Email,
        PasswordHash = request.Password,//todo
        PasswordSalt = request.Password,//todo
        DateFormat = "yyyy-MM-dd",
        TimeZone = "America/Vancouver",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        IsEnabled = true
      };

      _context.Users.Add(user);
      await _context.SaveChangesAsync(cancellationToken);

      user.DomainEvents.Add(new UserCreatedEvent(user));

      return user;
    }
  }
  
  public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
  {
    private readonly IAppDbContext _context;
    public CreateUserCommandValidator(IAppDbContext context)
    {
      _context = context;

      RuleFor(x => x.Name)
          .NotEmpty()
          .WithMessage("Name is required.");

      RuleFor(x => x.Email)
          .NotEmpty()
          .EmailAddress()
          .WithMessage("Valid email is required.")
          .MustAsync(BeUniqueEmail)
          .WithMessage("Email already exists.");

      RuleFor(x => x.Password)
          .NotEmpty()
          .MinimumLength(6)
          .WithMessage("Password must be at least 6 characters long.");
    }

    private Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
      return _context.Users
          .AllAsync(l => l.Email != email, cancellationToken);
    }
  }
}