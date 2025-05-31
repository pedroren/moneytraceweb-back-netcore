namespace MoneyTrace.Application.Features.Budgets;

using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

public record UpdateBudgetCommand(
    int UserId,
    int BudgetId,
    string Name,
    decimal Amount,
    DateTime StartDate,
    DateTime EndDate,
    BudgetCategoryModel[] BudgetCategories) : IRequest<ErrorOr<BudgetEntity>>;
public class UpdateBudgetCommandHandler : IRequestHandler<UpdateBudgetCommand, ErrorOr<BudgetEntity>>
{
    private readonly AppDbContext _context;

    public UpdateBudgetCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<BudgetEntity>> Handle(UpdateBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _context.Budgets
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.UserId == request.UserId && b.Id == request.BudgetId, cancellationToken);

        if (budget is null)
        {
            return Error.NotFound("Budget not found.");
        }

        budget.Name = request.Name;
        budget.Amount = request.Amount;
        budget.StartDate = request.StartDate;
        budget.EndDate = request.EndDate;
        budget.Categories = request.BudgetCategories.Select(c => new BudgetCategoryEntity
        {
            CategoryId = c.CategoryId,
            Amount = c.Amount
        }).ToList();
        budget.UpdatedAt = DateTime.UtcNow;

        _context.Budgets.Update(budget);
        await _context.SaveChangesAsync(cancellationToken);

        return budget;
    }
}
internal sealed class UpdateBudgetCommandValidator : AbstractValidator<UpdateBudgetCommand>
{
    private readonly AppDbContext _context;

    public UpdateBudgetCommandValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0.");

        RuleFor(x => x.BudgetId)
            .GreaterThan(0).WithMessage("Budget ID must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Budget name cannot be empty.")
            .MaximumLength(100).WithMessage("Budget name cannot exceed 100 characters.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Budget amount must be greater than 0.");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("Start date must be before or equal to end date.");

        RuleForEach(x => x.BudgetCategories)
            .ChildRules(categories =>
            {
                categories.RuleFor(c => c.CategoryId)
                    .GreaterThan(0).WithMessage("Category ID must be greater than 0.");
                categories.RuleFor(c => c.Amount)
                    .GreaterThan(0).WithMessage("Category amount must be greater than 0.");
            });
    }
}