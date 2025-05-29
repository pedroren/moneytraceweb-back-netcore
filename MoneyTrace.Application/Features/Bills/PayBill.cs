using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Operations;
using MoneyTrace.Application.Features.Templates;
using MoneyTrace.Application.Infraestructure.Persistence;

namespace MoneyTrace.Application.Features.Bills;

public record PayBillCommand(int UserId, int BillId, DateTime PaymentDate, decimal Amount, string Comments) : IRequest<ErrorOr<BillEntity>>;
public class PayBillCommandHandler : IRequestHandler<PayBillCommand, ErrorOr<BillEntity>>
{
    private readonly AppDbContext _context;
    private readonly IMediator _mediator;

    public PayBillCommandHandler(AppDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }


    public async Task<ErrorOr<BillEntity>> Handle(PayBillCommand request, CancellationToken cancellationToken)
    {
        var bill = await _context.Bills
            .FirstOrDefaultAsync(b => b.Id == request.BillId && b.UserId == request.UserId, cancellationToken);

        if (bill is null)
        {
            return Error.NotFound("Bill not found.");
        }

        var template = await _context.Templates
            .FirstOrDefaultAsync(t => t.Id == bill.TemplateId && t.UserId == request.UserId, cancellationToken);

        if (template is null)
        {
            return Error.NotFound("Template not found for this bill.");
        }

        //1) Create Operation Record
        var operationQuery = await _mediator.Send(
            new GetNewOperationFromTemplateQuery(request.UserId, bill.TemplateId), cancellationToken);
        if (operationQuery.IsError)
        {
            return operationQuery.Errors;
        }
        // Customize the operation for the payment
        var operation = operationQuery.Value with
        {
            Date = request.PaymentDate,
            Title = $"Payment for {bill.Name}",
            TotalAmount = request.Amount,
            Comments = request.Comments,
            Allocation = operationQuery.Value.Allocation.Select(a => new OperationCategoryModel(
                a.CategoryId,
                a.SubCategoryId,
                request.Amount)).ToArray() // Set the first allocation amount to the payment amount, only supports 1
        };
        
        var createOperation = await _mediator.Send(operation, cancellationToken);
        if (createOperation.IsError)
        {
            return createOperation.Errors;
        }

        //2) If Paid full, update Next Due Date, 3) Update Amount Due
        if (bill.NextDueAmount <= request.Amount)
        {
            // If the payment covers the full amount due, reset the bill
            bill.NextDueDate = bill.PaymentFrequency switch
            {
                Frequency.Weekly => bill.NextDueDate.AddDays(7),
                Frequency.BiWeekly => bill.NextDueDate.AddDays(14),
                Frequency.Monthly => bill.NextDueDate.AddMonths(1),
                Frequency.BiMonthly => bill.NextDueDate.AddMonths(1),
                Frequency.Yearly => bill.NextDueDate.AddYears(1),
                _ => bill.NextDueDate
            };
            bill.NextDueAmount = 0; // Reset to 0 since it's fully paid
        }
        else
        {
            // If not fully paid, reduce the amount due
            bill.NextDueAmount -= request.Amount;
        }
        return bill;
    }
}
internal sealed class PayBillCommandValidator : AbstractValidator<PayBillCommand>
{
    private readonly AppDbContext _context;

    public PayBillCommandValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.BillId)
            .GreaterThan(0).WithMessage("Bill ID must be greater than 0.")
            .MustAsync(BeExistingBill).WithMessage("The specified bill does not exist for this user.");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Payment date is required.")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Payment date cannot be in the future.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");
    }

    private async Task<bool> BeExistingBill(int billId, CancellationToken cancellationToken)
    {
        return await _context.Bills.AnyAsync(b => b.Id == billId, cancellationToken);
    }
}