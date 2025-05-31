namespace MoneyTrace.Application.Features.Budgets;

using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

public record DeleteBudgetCommand(int UserId, int BudgetId) : IRequest<ErrorOr<Unit>>;
public class DeleteBudgetCommandHandler : IRequestHandler<DeleteBudgetCommand, ErrorOr<Unit>>
{
    private readonly AppDbContext _context;

    public DeleteBudgetCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Unit>> Handle(DeleteBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _context.Budgets
            .FirstOrDefaultAsync(b => b.UserId == request.UserId && b.Id == request.BudgetId, cancellationToken);

        if (budget is null)
        {
            return Error.NotFound("Budget not found.");
        }

        _context.Budgets.Remove(budget);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
internal sealed class DeleteBudgetCommandValidator : AbstractValidator<DeleteBudgetCommand>
{
    public DeleteBudgetCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0.");

        RuleFor(x => x.BudgetId)
            .GreaterThan(0).WithMessage("Budget ID must be greater than 0.");
    }
}