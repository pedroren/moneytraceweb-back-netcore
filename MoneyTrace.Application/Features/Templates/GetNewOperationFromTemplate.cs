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

public record GetNewOperationForPaymentQuery(int UserId, int TemplateId, DateTime PaymentDate, string Title, decimal Amount, string Comments) 
    : IRequest<ErrorOr<CreateOperationCommand>>;
public class GetNewOperationForPaymentQueryHandler : IRequestHandler<GetNewOperationForPaymentQuery, ErrorOr<CreateOperationCommand>>
{
    private readonly IMediator _mediator;

    public GetNewOperationForPaymentQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<ErrorOr<CreateOperationCommand>> Handle(GetNewOperationForPaymentQuery request, CancellationToken cancellationToken)
    {
        var operationQuery = await _mediator.Send(
            new GetNewOperationFromTemplateQuery(request.UserId, request.TemplateId), cancellationToken);
        
        if (operationQuery.IsError)
        {
            return operationQuery.Errors;
        }

        // Customize the operation for the payment
        var operation = operationQuery.Value with
        {
            Date = request.PaymentDate,
            Title = request.Title,
            TotalAmount = request.Amount,
            Comments = request.Comments,
            Allocation = operationQuery.Value.Allocation.Select(a => new OperationCategoryModel(
                a.CategoryId,
                a.SubCategoryId,
                request.Amount)).ToArray() // Set the first allocation amount to the payment amount, only supports 1
        };

        return operation;
    }
}