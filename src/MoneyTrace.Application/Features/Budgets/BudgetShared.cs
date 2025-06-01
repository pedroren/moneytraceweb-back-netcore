public record BudgetCategoryModel(
    int Id,
    int CategoryId,
    decimal Amount);

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