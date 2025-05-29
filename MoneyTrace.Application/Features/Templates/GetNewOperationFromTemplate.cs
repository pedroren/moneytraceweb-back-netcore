namespace MoneyTrace.Application.Features.Templates;

using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Common;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Operations;
using MoneyTrace.Application.Infraestructure.Persistence;

public record GetNewOperationFromTemplateQuery(int UserId, int TemplateId) : IRequest<ErrorOr<CreateOperationCommand>>;
public class GetNewOperationFromTemplateQueryHandler : IRequestHandler<GetNewOperationFromTemplateQuery, ErrorOr<CreateOperationCommand>>
{
    private readonly AppDbContext _context;

    public GetNewOperationFromTemplateQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<CreateOperationCommand>> Handle(GetNewOperationFromTemplateQuery request, CancellationToken cancellationToken)
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

        var operation = new CreateOperationCommand(request.UserId, DateTime.Today, template.Title, template.Type, template.VendorId,
            template.AccountId, template.DestinationAccountId, template.TotalAmount, string.Empty, template.CategoryType,
            template.Allocation.Select(a => new OperationCategoryModel(a.CategoryId, a.SubCategoryId, a.Amount)).ToArray()
            );
        
        return operation;
    }
}