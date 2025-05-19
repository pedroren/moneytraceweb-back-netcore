namespace MoneyTrace.Application.Features.Categories;

using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

public record DeleteCategoryCommand(int UserId, int CategoryId) : IRequest<ErrorOr<CategoryEntity>>;
public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, ErrorOr<CategoryEntity>>
{
    private readonly AppDbContext _context;

    public DeleteCategoryCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<CategoryEntity>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
          .Include(x => x.SubCategories)
          .FirstOrDefaultAsync(x => x.Id == request.CategoryId && x.UserId == request.UserId, cancellationToken);

        if (category == null)
        {
            return Error.NotFound("Category not found.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);

        return category;
    }
}

internal sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    private readonly AppDbContext _context;

    public DeleteCategoryCommandValidator(AppDbContext context)
    {
        _context = context;
        RuleFor(x => x.UserId)
          .GreaterThan(0).WithMessage("User not identified.");
        RuleFor(x => x.CategoryId)
          .GreaterThan(0).WithMessage("Category not identified.");
        //TODO: Validate if the category is not used in any transaction
                  
    }
}



