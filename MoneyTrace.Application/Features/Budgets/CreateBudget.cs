namespace MoneyTrace.Application.Features.Budgets;

using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

public record CreateBudgetCommand(
    int UserId,
    string Name,
    decimal Amount,
    DateTime StartDate,
    DateTime EndDate,
    BudgetCategoryEntity[] BudgetCategories) : IRequest<ErrorOr<BudgetEntity>>;
public class CreateBudgetCommandHandler : IRequestHandler<CreateBudgetCommand, ErrorOr<BudgetEntity>>
{
    private readonly AppDbContext _context;

    public CreateBudgetCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<BudgetEntity>> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = new BudgetEntity
        {
            UserId = request.UserId,
            Name = request.Name,
            Amount = request.Amount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Frequency = Frequency.Monthly, // Default frequency
            Categories = request.BudgetCategories.ToList(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync(cancellationToken);

        return budget;
    }
}
internal sealed class CreateBudgetCommandValidator : AbstractValidator<CreateBudgetCommand>
{
    private readonly AppDbContext _context;

    public CreateBudgetCommandValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters.")
            .MustAsync(BeUniqueName)
            .WithMessage("A budget with the same name already exists.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .WithMessage("Start date must be before or equal to end date.");

        //Validate that the dates ranges dont' overlap with existing budgets
        RuleFor(x => x.StartDate)
            .MustAsync(async (command, startDate, cancellationToken) =>
            {
                return !await _context.Budgets
                    .AnyAsync(b => b.UserId == command.UserId &&
                                   b.StartDate < command.EndDate &&
                                   b.EndDate > command.StartDate,
                        cancellationToken);
            })
            .WithMessage("The budget dates overlap with an existing budget.");
        RuleFor(x => x.BudgetCategories)
            .NotEmpty()
            .WithMessage("At least one budget category is required.");
    }

    private async Task<bool> BeUniqueName(CreateBudgetCommand command, string name, CancellationToken cancellationToken)
    {
        return !await _context.Budgets
            .AnyAsync(b => b.Name == name, cancellationToken);
    }
}

/// <summary>
/// Create a new budget for the next period based on an existing budget.
/// This command is used to create a new budget that will be used for the next period, typically after the current budget has ended.
/// </summary>
/// <param name="UserId"></param>
/// <param name="BudgetId"></param>
public record CreateNextBudgetCommand(
    int UserId,
    int BudgetId) : IRequest<ErrorOr<BudgetEntity>>;
public class CreateNextBudgetCommandHandler : IRequestHandler<CreateNextBudgetCommand, ErrorOr<BudgetEntity>>
{
    private readonly AppDbContext _context;
    private readonly IMediator _mediator;

    public CreateNextBudgetCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<BudgetEntity>> Handle(CreateNextBudgetCommand request, CancellationToken cancellationToken)
    {
        var existingBudget = await _context.Budgets
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.Id == request.BudgetId && b.UserId == request.UserId, cancellationToken);

        if (existingBudget is null)
        {
            return Error.NotFound("Budget not found.");
        }

        var startDate = existingBudget.EndDate.AddDays(1);
        var endDate = startDate.AddMonths(1); // Assuming monthly frequency for the next budget
        var nextBudget = new CreateBudgetCommand(
            UserId: request.UserId,
            Name: $"{existingBudget.Name} - {startDate:'MMM'}-{startDate.Year}",
            Amount: existingBudget.Amount,
            StartDate: startDate,
            EndDate: endDate, // Assuming monthly frequency
            BudgetCategories: existingBudget.Categories.Select(c => new BudgetCategoryEntity
            {
                CategoryId = c.CategoryId,
                Amount = c.Amount
            }).ToArray());        

        var createBudget = await _mediator.Send(nextBudget, cancellationToken);
        if (createBudget.IsError)
        {
            return createBudget.Errors;
        }

        return createBudget.Value;
    }
}
internal sealed class CreateNextBudgetCommandValidator : AbstractValidator<CreateNextBudgetCommand>
{
    private readonly AppDbContext _context;

    public CreateNextBudgetCommandValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.BudgetId)
            .GreaterThan(0)
            .WithMessage("Budget ID must be greater than zero.")
            .MustAsync(BeExistingBudget)
            .WithMessage("The specified budget does not exist for the user.");
    }

    private async Task<bool> BeExistingBudget(CreateNextBudgetCommand command, int budgetId, CancellationToken cancellationToken)
    {
        return await _context.Budgets
            .AnyAsync(b => b.Id == budgetId && b.UserId == command.UserId, cancellationToken);
    }
}
