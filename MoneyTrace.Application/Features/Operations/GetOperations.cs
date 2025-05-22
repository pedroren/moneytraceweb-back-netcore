namespace MoneyTrace.Application.Features.Operations;

using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

public record GetOperationsQuery(int UserId) : IRequest<ErrorOr<List<OperationEntity>>>;
public class GetOperationsQueryHandler : IRequestHandler<GetOperationsQuery, ErrorOr<List<OperationEntity>>>
{
    private readonly AppDbContext _context;

    public GetOperationsQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<OperationEntity>>> Handle(GetOperationsQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId <= 0)
        {
            return Error.Unauthorized("User not identified.");
        }
        return await _context.Operations.AsNoTracking()
          .Include(x => x.Categories)
          .ThenInclude(x => x.Category)
          .ThenInclude(x => x.SubCategories)
          .Include(x => x.Categories)
          .ThenInclude(x => x.SubCategory)
          .Where(x => x.UserId == request.UserId)
          .ToListAsync(cancellationToken);
    }
}
public record GetOperationByIdQuery(int UserId, int OperationId) : IRequest<ErrorOr<OperationEntity>>;
public class GetOperationByIdQueryHandler : IRequestHandler<GetOperationByIdQuery, ErrorOr<OperationEntity>>
{
    private readonly AppDbContext _context;

    public GetOperationByIdQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<OperationEntity>> Handle(GetOperationByIdQuery request, CancellationToken cancellationToken)
    {
        var operation = await _context.Operations.AsNoTracking()
          .Include(x => x.Categories)
          .ThenInclude(x => x.Category)
          .ThenInclude(x => x.SubCategories)
          .Include(x => x.Categories)
          .ThenInclude(x => x.SubCategory)
          .FirstOrDefaultAsync(x => x.Id == request.OperationId && x.UserId == request.UserId, cancellationToken);

        if (operation == null)
        {
            return Error.NotFound("Operation not found.");
        }

        return operation;
    }
}
public record GetOperationByCriteriaQuery(int UserId, DateTime StartDate, DateTime EndDate, int? AccountId, int? CategoryId, int? VendorId, OperationType? Type) : IRequest<ErrorOr<List<OperationEntity>>>;
public class GetOperationByCriteriaQueryHandler : IRequestHandler<GetOperationByCriteriaQuery, ErrorOr<List<OperationEntity>>>
{
    private readonly AppDbContext _context;

    public GetOperationByCriteriaQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<OperationEntity>>> Handle(GetOperationByCriteriaQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId <= 0)
        {
            return Error.Unauthorized("User not identified.");
        }

        var query = _context.Operations.AsNoTracking()
            .Include(x => x.Categories)
            .ThenInclude(x => x.Category)
            .ThenInclude(x => x.SubCategories)
            .Where(x => x.UserId == request.UserId);

        if (request.StartDate != DateTime.MinValue && request.EndDate != DateTime.MinValue)
        {
            query = query.Where(x => x.Date >= request.StartDate && x.Date <= request.EndDate);
        }

        if (request.AccountId.HasValue)
        {
            query = query.Where(x => x.Account.Id == request.AccountId.Value);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(x => x.Categories.Any(c => c.Category.Id == request.CategoryId.Value));
        }

        if (request.VendorId.HasValue)
        {
            query = query.Where(x => x.Vendor.Id == request.VendorId.Value);
        }

        if (request.Type.HasValue)
        {
            query = query.Where(x => x.Type == request.Type);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
public sealed class GetOperationByCriteriaQueryValidator : AbstractValidator<GetOperationByCriteriaQuery>
{
    public GetOperationByCriteriaQueryValidator()
    {
        RuleFor(x => x.UserId)
          .GreaterThan(0).WithMessage("User not identified.");
        RuleFor(x => x.StartDate)
          .NotEmpty().WithMessage("Start date is required.");
        RuleFor(x => x.EndDate)
          .NotEmpty().WithMessage("End date is required.");
    }
}
