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

public record GetSubCategoryByIdQuery(int UserId, int SubCategoryId) : IRequest<ErrorOr<SubCategoryEntity>>;
public class GetSubCategoryByIdQueryHandler : IRequestHandler<GetSubCategoryByIdQuery, ErrorOr<SubCategoryEntity>>
{
    private readonly AppDbContext _context;

    public GetSubCategoryByIdQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<SubCategoryEntity>> Handle(GetSubCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var subCategory = await _context.SubCategories.AsNoTracking()
          //.Include(x => x.Category)
          .FirstOrDefaultAsync(x => x.Id == request.SubCategoryId && x.Category.UserId == request.UserId, cancellationToken);

        if (subCategory == null)
        {
            return Error.NotFound("Subcategory not found.");
        }

        return subCategory;
    }
}


