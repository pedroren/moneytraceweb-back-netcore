namespace MoneyTrace.Application.Features.Categories;

using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

public record GetUserCategoriesQuery(int UserId) : IRequest<ErrorOr<List<CategoryEntity>>>;
public class GetUserCategoriesQueryHandler : IRequestHandler<GetUserCategoriesQuery, ErrorOr<List<CategoryEntity>>>
{
    private readonly AppDbContext _context;

    public GetUserCategoriesQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<CategoryEntity>>> Handle(GetUserCategoriesQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId <= 0)
        {
            return Error.Unauthorized("User not identified.");
        }
        return await _context.Categories.AsNoTracking()
          .Include(x => x.SubCategories)
          .Where(x => x.UserId == request.UserId)
          .ToListAsync(cancellationToken);
    }
}

public record GetCategoryByIdQuery(int UserId, int CategoryId) : IRequest<ErrorOr<CategoryEntity>>;
public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, ErrorOr<CategoryEntity>>
{
    private readonly AppDbContext _context;

    public GetCategoryByIdQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<CategoryEntity>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.AsNoTracking()
          .Include(x => x.SubCategories)
          .FirstOrDefaultAsync(x => x.Id == request.CategoryId && x.UserId == request.UserId, cancellationToken);

        if (category == null)
        {
            return Error.NotFound("Category not found.");
        }

        return category;
    }
}


