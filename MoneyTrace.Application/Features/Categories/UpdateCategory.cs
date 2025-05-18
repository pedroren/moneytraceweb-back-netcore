namespace MoneyTrace.Application.Features.Categories;

using System;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

public record UpdateSubCategoryCommand(int Id, int CategoryId, string Name, bool IsEnabled) : IRequest<ErrorOr<SubCategoryEntity>>;
public record UpdateCategoryCommand(int UserId, int CategoryId, string Name, CategoryType Type, bool IsEnabled, UpdateSubCategoryCommand[] SubCategories) : IRequest<ErrorOr<CategoryEntity>>;
public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, ErrorOr<CategoryEntity>>
{
    private readonly AppDbContext _context;

    public UpdateCategoryCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<CategoryEntity>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
          .Include(x => x.SubCategories)
          .FirstOrDefaultAsync(x => x.Id == request.CategoryId && x.UserId == request.UserId, cancellationToken);

        if (category == null)
        {
            return Error.NotFound("Category not found.");
        }

        category.Name = request.Name;
        category.Type = request.Type;
        category.IsEnabled = request.IsEnabled;
        UpdateSubCatgories(category, request.SubCategories);

        await _context.SaveChangesAsync(cancellationToken);

        return category;
    }

    private void UpdateSubCatgories(CategoryEntity category, UpdateSubCategoryCommand[] subCategories)
    {
        // Remove subcategories that are not in the new list
        foreach (var subCategory in category.SubCategories.ToList())
        {
            if (!subCategories.Any(x => x.Id == subCategory.Id))
            {
                category.SubCategories.Remove(subCategory);
            }
        }
        // Add or update subcategories    
        foreach (var subCategory in subCategories)
        {
            var existingSubCategory = category.SubCategories.FirstOrDefault(x => x.Name == subCategory.Name);
            if (existingSubCategory != null)
            {
                existingSubCategory.Name = subCategory.Name;
                existingSubCategory.IsEnabled = subCategory.IsEnabled;
            }
            else
            {
                category.SubCategories.Add(new SubCategoryEntity()
                {
                    Name = subCategory.Name,
                    IsEnabled = subCategory.IsEnabled
                });
            }
        }
    }
}
internal sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    private readonly AppDbContext _context;

    public UpdateCategoryCommandValidator(AppDbContext context)
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