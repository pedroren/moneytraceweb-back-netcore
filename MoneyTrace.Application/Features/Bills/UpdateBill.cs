using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

namespace MoneyTrace.Application.Features.Bills;

public record UpdateBillCommand(
    int UserId,
    int BillId,
    string Name,
    int TemplateId,
    Frequency PaymentFrequency,
    DateTime NextDueDate,
    decimal NextDueAmount,
    int PaymentDay,
    int? PaymentMonth) : IRequest<ErrorOr<BillEntity>>;
public class UpdateBillCommandHandler : IRequestHandler<UpdateBillCommand, ErrorOr<BillEntity>>
{
    private readonly AppDbContext _context;

    public UpdateBillCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<BillEntity>> Handle(UpdateBillCommand request, CancellationToken cancellationToken)
    {
        var bill = await _context.Bills
            .FirstOrDefaultAsync(b => b.Id == request.BillId && b.UserId == request.UserId, cancellationToken);

        if (bill is null)
        {
            return Error.NotFound("Bill not found.");
        }

        bill.Name = request.Name;
        bill.TemplateId = request.TemplateId;
        bill.PaymentFrequency = request.PaymentFrequency;
        bill.NextDueDate = request.NextDueDate;
        bill.NextDueAmount = request.NextDueAmount;
        bill.PaymentDay = request.PaymentDay;
        bill.PaymentMonth = request.PaymentMonth;

        _context.Bills.Update(bill);
        await _context.SaveChangesAsync(cancellationToken);

        return bill;
    }
}

internal sealed class UpdateBillCommandValidator : AbstractValidator<UpdateBillCommand>
{
    private readonly AppDbContext _context;

    public UpdateBillCommandValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.BillId)
            .GreaterThan(0).WithMessage("Bill ID must be greater than 0.")
            .MustAsync(BeExistingBill).WithMessage("The specified bill does not exist for this user.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
            .MustAsync((m, name, canToken) => BeUniqueName(m.UserId, m.BillId, name, CancellationToken.None))
            .WithMessage("A bill with the same name already exists for this user.");

        RuleFor(x => x.TemplateId)
            .GreaterThan(0).WithMessage("Template ID must be greater than 0.")
            .MustAsync(async (request, templateId, cancellationToken) =>
            {
                return await _context.Templates.AnyAsync(t => t.Id == templateId && t.UserId == request.UserId, cancellationToken);
            }).WithMessage("Template does not exist for this user.");
    }

    private async Task<bool> BeExistingBill(int billId, CancellationToken cancellationToken)
    {
        return await _context.Bills.AnyAsync(b => b.Id == billId, cancellationToken);
    }

    private async Task<bool> BeUniqueName(int userId, int billId, string name, CancellationToken cancellationToken)
    {
        return !await _context.Bills.AnyAsync(b => b.UserId == userId && b.Name == name && b.Id != billId, cancellationToken);
    }
}