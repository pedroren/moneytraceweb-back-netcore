using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

namespace MoneyTrace.Application.Features.Bills;

public record CreateBillCommand(int UserId, string Name, int TemplateId, Frequency PaymentFrequency, DateTime NextDueDate, decimal NextDueAmount, int PaymentDay, int? PaymentMonth) : IRequest<ErrorOr<BillEntity>>;
public class CreateBillCommandHandler : IRequestHandler<CreateBillCommand, ErrorOr<BillEntity>>
{
    private readonly AppDbContext _context;

    public CreateBillCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<BillEntity>> Handle(CreateBillCommand request, CancellationToken cancellationToken)
    {
        var bill = new BillEntity
        {
            UserId = request.UserId,
            Name = request.Name,
            TemplateId = request.TemplateId,
            PaymentFrequency = request.PaymentFrequency,
            NextDueDate = request.NextDueDate,
            NextDueAmount = request.NextDueAmount,
            PaymentDay = request.PaymentDay,
            PaymentMonth = request.PaymentMonth,
            IsEnabled = true,
        };

        _context.Bills.Add(bill);
        await _context.SaveChangesAsync(cancellationToken);

        return bill;
    }
}
internal sealed class CreateBillCommandValidator : AbstractValidator<CreateBillCommand>
{
    private readonly AppDbContext _context;

    public CreateBillCommandValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
            .MustAsync((m, name, canToken) => BeUniqueName(m.UserId, name, CancellationToken.None))
            .WithMessage("A bill with the same name already exists for this user.");

        RuleFor(x => x.TemplateId)
            .GreaterThan(0).WithMessage("Template ID must be greater than 0.")
            .MustAsync(async (request, templateId, cancellationToken) =>
            {
                return await _context.Templates.AnyAsync(t => t.Id == templateId && t.UserId == request.UserId, cancellationToken);
            }).WithMessage("Template does not exists for this user.");

        RuleFor(x => x.NextDueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Next due date must be in the future.");

        RuleFor(x => x.NextDueAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Next due amount must be non-negative.");

        RuleFor(x => x.PaymentDay)
            .InclusiveBetween(1, 7).WithMessage("Payment day must be between 1 and 7 for weekly and biweekly payments.")
            .When(x => x.PaymentFrequency == Frequency.Weekly || x.PaymentFrequency == Frequency.BiWeekly);
        RuleFor(x => x.PaymentDay)
            .InclusiveBetween(1, 31).WithMessage("Payment day must be between 1 and 31.")
            .When(x => x.PaymentFrequency == Frequency.Monthly || x.PaymentFrequency == Frequency.BiMonthly || x.PaymentFrequency == Frequency.Yearly);

        RuleFor(x => x.PaymentMonth)
            .Must(BeValidPaymentMonth).When(x => x.PaymentFrequency == Frequency.Yearly)
            .WithMessage("Payment month must be between 1 and 12 for yearly payments.");
    }

    private async Task<bool> BeUniqueName(int userId, string name, CancellationToken cancellationToken)
    {
        return !await _context.Bills.AnyAsync(b => b.Name == name && b.UserId == userId, cancellationToken);
    }

    private bool BeValidPaymentMonth(int? paymentMonth)
    {
        return paymentMonth.HasValue && paymentMonth.Value >= 1 && paymentMonth.Value <= 12;
    }
}