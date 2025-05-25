namespace MoneyTrace.Application.Features.Templates;

using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Common;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Operations;
using MoneyTrace.Application.Infraestructure.Persistence;

public record GetNewOperationFromTemplateQuery(int UserId, int TemplateId) : IRequest<ErrorOr<OperationEntity>>;
public class GetNewOperationFromTemplateQueryHandler : IRequestHandler<GetNewOperationFromTemplateQuery, ErrorOr<OperationEntity>>
{
    private readonly AppDbContext _context;

    public GetNewOperationFromTemplateQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<OperationEntity>> Handle(GetNewOperationFromTemplateQuery request, CancellationToken cancellationToken)
    {
        var template = await _context.Templates
            .Include(t => t.Allocation)
            .ThenInclude(c => c.Category)
            .Include(t => t.Allocation)
            .ThenInclude(c => c.SubCategory)
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId && t.UserId == request.UserId, cancellationToken);

        if (template == null)
        {
            return Error.NotFound("Template not found.");
        }

        var operation = new OperationEntity
        {
            UserId = request.UserId,
            Title = template.Title,
            VendorId = template.VendorId,
            AccountId = template.AccountId,
            TotalAmount = template.TotalAmount,
            CategoryType = template.CategoryType,
            Allocation = template.Allocation.Select(a => new OperationCategoryEntity
            {
                CategoryId = a.CategoryId,
                SubCategoryId = a.SubCategoryId,
                Amount = a.Amount
            }).ToList()
        };

        return operation;
    }
}