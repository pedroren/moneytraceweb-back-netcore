namespace MoneyTrace.Application.Common;

using MoneyTrace.Application.Domain;
using MediatR;
using MoneyTrace.Application.Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

public record CreateCategoryCommand(int userId, string Name, CategoryType type) : IRequest<CategoryEntity>;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryEntity>
{
  private readonly IAppDbContext _context;

  public CreateCategoryCommandHandler(IAppDbContext context)
  {
    _context = context;
  }

  public async Task<CategoryEntity> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
  {
    var category = new CategoryEntity()
    {
      Name = request.Name,
      Type = request.type,
      UserId = request.userId,
      IsEnabled = true
    };
    await _context.Categories.AddAsync(category, cancellationToken);
    return category;
  }


}

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
  private readonly IAppDbContext _context;

  public CreateCategoryCommandValidator(IAppDbContext context)
  {
    _context = context;

    RuleFor(x => x.Name)
        .NotEmpty()
        .WithMessage("Name is required.")
        .MaximumLength(100)
        .WithMessage("Name must not exceed 100 characters.")
        .MustAsync(BeUniqueName)
        .WithMessage("An account with the same name already exists.");
    RuleFor(x => x.userId)
        .GreaterThan(0)
        .WithMessage("User not identified.");
  }

  private Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
  {
    return _context.Categories
        .AllAsync(x => x.Name != name, cancellationToken);
  }
}