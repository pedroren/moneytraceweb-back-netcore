namespace MoneyTrace.Application.Features.Categories;

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

public record CreateCategoryCommand(int UserId, string Name, CategoryType Type, string[] SubCategories) : IRequest<CategoryEntity>;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryEntity>
{
    private readonly AppDbContext _context;

    public CreateCategoryCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryEntity> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new CategoryEntity()
        {
            Name = request.Name,
            Type = request.Type,
            UserId = request.UserId,
            IsEnabled = true,
            SubCategories = request.SubCategories.Select(x => new SubCategoryEntity()
            {
                Name = x,
                IsEnabled = true
            }).ToList()
        };
        await _context.Categories.AddAsync(category, cancellationToken);
        return category;
    }
}

internal sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    private readonly AppDbContext _context;

    public CreateCategoryCommandValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters.")
            .MustAsync((m, name, canToken) => BeUniqueName(m.UserId, name, CancellationToken.None))
            .WithMessage("An account with the same name already exists.");
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User not identified.");
        RuleFor(x => x.SubCategories)
            .NotEmpty()
            .WithMessage("At least one subcategory is required.")
            .ForEach(x => x.NotEmpty().WithMessage("Subcategory name is required."));

    }

    private async Task<bool> BeUniqueName(int userId, string name, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .AllAsync(x => x.UserId == userId && x.Name != name, cancellationToken);
    }
}