using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

namespace MoneyTrace.Application.Features.Bills;

public record DeleteBillCommand(int UserId, int BillId) : IRequest<ErrorOr<BillEntity>>;
public class DeleteBillCommandHandler : IRequestHandler<DeleteBillCommand, ErrorOr<BillEntity>>
{
    private readonly AppDbContext _context;

    public DeleteBillCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<BillEntity>> Handle(DeleteBillCommand request, CancellationToken cancellationToken)
    {
        var bill = await _context.Bills
            .FirstOrDefaultAsync(b => b.Id == request.BillId && b.UserId == request.UserId, cancellationToken);

        if (bill is null)
        {
            return Error.NotFound("Bill not found.");
        }

        _context.Bills.Remove(bill);
        await _context.SaveChangesAsync(cancellationToken);

        return bill;
    }
}
internal sealed class DeleteBillCommandValidator : AbstractValidator<DeleteBillCommand>
{
    private readonly AppDbContext _context;

    public DeleteBillCommandValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.BillId)
            .GreaterThan(0).WithMessage("Bill ID must be greater than 0.")
            .MustAsync(BeExistingBill).WithMessage("The specified bill does not exist for this user.");
    }

    private async Task<bool> BeExistingBill(int billId, CancellationToken cancellationToken)
    {
        return await _context.Bills.AnyAsync(b => b.Id == billId, cancellationToken);
    }
}