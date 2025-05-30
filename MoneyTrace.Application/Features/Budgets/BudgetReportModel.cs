namespace MoneyTrace.Application.Features.Budgets;

public record BudgetReportModel(
    int UserId,
    int BudgetId,
    string BudgetName,
    DateTime StartDate,
    DateTime EndDate,
    List<BudgetCategoryReportModel> Categories);
public record BudgetCategoryReportModel(
    int CategoryId,
    string CategoryName,
    decimal BudgetedAmount,
    decimal SpentAmount,
    decimal RemainingAmount);