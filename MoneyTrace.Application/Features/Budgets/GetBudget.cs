namespace MoneyTrace.Application.Features.Budgets;

using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

public record GetBudgetsQuery(int UserId) : IRequest<ErrorOr<List<BudgetEntity>>>;
public class GetBudgetsQueryHandler : IRequestHandler<GetBudgetsQuery, ErrorOr<List<BudgetEntity>>>
{
    private readonly AppDbContext _context;

    public GetBudgetsQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<BudgetEntity>>> Handle(GetBudgetsQuery request, CancellationToken cancellationToken)
    {
        var budgets = await _context.Budgets
            .AsNoTracking()
            .Where(b => b.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        if (budgets is null || !budgets.Any())
        {
            return Error.NotFound("No budgets found for this user.");
        }

        return budgets;
    }
}

public record GetBudgetQuery(int UserId, int BudgetId) : IRequest<ErrorOr<BudgetEntity>>;
public class GetBudgetQueryHandler : IRequestHandler<GetBudgetQuery, ErrorOr<BudgetEntity>>
{
    private readonly AppDbContext _context;

    public GetBudgetQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<BudgetEntity>> Handle(GetBudgetQuery request, CancellationToken cancellationToken)
    {
        var budget = await _context.Budgets
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.UserId == request.UserId && b.Id == request.BudgetId, cancellationToken);

        if (budget is null)
        {
            return Error.NotFound("Budget not found.");
        }

        return budget;
    }
}
internal sealed class GetBudgetQueryValidator : AbstractValidator<GetBudgetQuery>
{
    private readonly AppDbContext _context;

    public GetBudgetQueryValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0.");

        RuleFor(x => x.BudgetId)
            .GreaterThan(0)
            .WithMessage("Budget ID must be greater than 0.");
    }
}

//Get budget valid for a date
public record GetCurrentBudgetQuery(int UserId, DateTime Date) : IRequest<ErrorOr<BudgetEntity>>;
public class GetCurrentBudgetQueryHandler : IRequestHandler<GetCurrentBudgetQuery, ErrorOr<BudgetEntity>>
{
    private readonly AppDbContext _context;

    public GetCurrentBudgetQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<BudgetEntity>> Handle(GetCurrentBudgetQuery request, CancellationToken cancellationToken)
    {
        var budget = await _context.Budgets
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.UserId == request.UserId && b.StartDate <= request.Date && b.EndDate >= request.Date, cancellationToken);

        if (budget is null)
        {
            return Error.NotFound("No budget found for the specified date.");
        }

        return budget;
    }
}
internal sealed class GetCurrentBudgetQueryValidator : AbstractValidator<GetCurrentBudgetQuery>
{
    private readonly AppDbContext _context;

    public GetCurrentBudgetQueryValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0.");

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Date must be provided.");
    }
}

//Get budget vs spent report
public record GetBudgetReportQuery(int UserId, int BudgetId) : IRequest<ErrorOr<BudgetReportModel>>;
public class GetBudgetReportQueryHandler : IRequestHandler<GetBudgetReportQuery, ErrorOr<BudgetReportModel>>
{
    private readonly AppDbContext _context;

    public GetBudgetReportQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<BudgetReportModel>> Handle(GetBudgetReportQuery request, CancellationToken cancellationToken)
    {
        var budget = await _context.Budgets
            .AsNoTracking()
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.UserId == request.UserId && b.Id == request.BudgetId, cancellationToken);

        if (budget is null)
        {
            return Error.NotFound("Budget not found.");
        }

        var reportCategories = new List<BudgetCategoryReportModel>();
        foreach (var category in budget.Categories)
        {
            var spent = await _context.Operations.AsNoTracking()
                .Where(t => t.UserId == request.UserId && t.Date >= budget.StartDate && t.Date <= budget.EndDate && t.Allocation.Any(a => a.CategoryId == category.CategoryId))
                .SelectMany(t => t.Allocation)
                .Where(a => a.CategoryId == category.CategoryId)
                .SumAsync(t => t.Amount, cancellationToken);
            reportCategories.Add(new BudgetCategoryReportModel(category.CategoryId, category.Category.Name, category.Amount, spent, category.Amount - spent));
        }

        var report = new BudgetReportModel(
            budget.Id,
            budget.UserId,
            budget.Name,
            budget.StartDate,
            budget.EndDate,
            reportCategories
        );

        return report;
    }
}
internal sealed class GetBudgetReportQueryValidator : AbstractValidator<GetBudgetReportQuery>
{
    private readonly AppDbContext _context;

    public GetBudgetReportQueryValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0.");

        RuleFor(x => x.BudgetId)
            .GreaterThan(0)
            .WithMessage("Budget ID must be greater than 0.");
    }
}