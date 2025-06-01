namespace MoneyTrace.Application.Features.Templates;

using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Common;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Operations;
using MoneyTrace.Application.Infraestructure.Persistence;

public record GetTemplatesQuery(int UserId) : IRequest<ErrorOr<List<TemplateEntity>>>;
public class GetTemplatesQueryHandler : IRequestHandler<GetTemplatesQuery, ErrorOr<List<TemplateEntity>>>
{
    private readonly AppDbContext _context;

    public GetTemplatesQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<TemplateEntity>>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId <= 0)
        {
            return Error.Unauthorized("User not identified.");
        }
        return await _context.Templates.AsNoTracking()
          .Include(x => x.Allocation)
          .ThenInclude(x => x.Category)
          .ThenInclude(x => x.SubCategories)
          .Include(x => x.Allocation)
          .ThenInclude(x => x.SubCategory)
          .Where(x => x.UserId == request.UserId)
          .ToListAsync(cancellationToken);
    }
}

public record GetTemplateByIdQuery(int UserId, int TemplateId) : IRequest<ErrorOr<TemplateEntity>>;
public class GetTemplateByIdQueryHandler : IRequestHandler<GetTemplateByIdQuery, ErrorOr<TemplateEntity>>
{
    private readonly AppDbContext _context;

    public GetTemplateByIdQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<TemplateEntity>> Handle(GetTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await _context.Templates.AsNoTracking()
          .Include(x => x.Allocation)
          .ThenInclude(x => x.Category)
          .ThenInclude(x => x.SubCategories)
          .Include(x => x.Allocation)
          .ThenInclude(x => x.SubCategory)
          .FirstOrDefaultAsync(x => x.Id == request.TemplateId && x.UserId == request.UserId, cancellationToken);

        if (template == null)
        {
            return Error.NotFound("Template not found.");
        }
        return template;
    }
}