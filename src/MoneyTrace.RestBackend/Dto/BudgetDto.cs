using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Budgets;

namespace MoneyTrace.RestBackend.Dto;

public record BudgetDto(
    int Id,
    string Name,
    decimal Amount,
    DateTime StartDate,
    DateTime EndDate,
    int UserId,
    BudgetCategoryModel[] BudgetCategories);
public static class BudgetDtoExtensions
{
    public static UpdateBudgetCommand ToUpdateCommand(this BudgetDto dto, int id, int userId)
    {
        return new UpdateBudgetCommand(
            userId,
            id,
            dto.Name.Trim(),
            dto.Amount,
            dto.StartDate,
            dto.EndDate,
            dto.BudgetCategories);
    }

    public static CreateBudgetCommand ToCreateCommand(this BudgetDto dto, int userId)
    {
        return new CreateBudgetCommand(
            userId,
            dto.Name.Trim(),
            dto.Amount,
            dto.StartDate,
            dto.EndDate,
            dto.BudgetCategories);
    }

    public static BudgetDto ToDto(this BudgetEntity budget)
    {
        return new BudgetDto(
            budget.Id,
            budget.Name,
            budget.Amount,
            budget.StartDate,
            budget.EndDate,
            budget.UserId,
            budget.Categories.Select(c => new BudgetCategoryModel(budget.Id, c.CategoryId, c.Amount)).ToArray());
    }

    public static BudgetEntity ToEntity(this BudgetDto budgetDto)
    {
        return new BudgetEntity
        {
            Id = budgetDto.Id,
            Name = budgetDto.Name.Trim(),
            Amount = budgetDto.Amount,
            StartDate = budgetDto.StartDate,
            EndDate = budgetDto.EndDate,
            UserId = budgetDto.UserId,
            Categories = budgetDto.BudgetCategories.Select(c => new BudgetCategoryEntity
            {
                CategoryId = c.CategoryId,
                Amount = c.Amount
            }).ToList()
        };
    }
}