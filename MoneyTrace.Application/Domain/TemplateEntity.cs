using System.ComponentModel.DataAnnotations;
using MoneyTrace.Application.Common;

namespace MoneyTrace.Application.Domain;

/// Operations Template, for simple operation type
public class TemplateEntity : AuditableEntity
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    public OperationType Type { get; set; } = OperationType.Simple;
    public int? VendorId { get; set; }
    public int AccountId { get; set; }
    public int? DestinationAccountId { get; set; } // For transfers, null for simple operations
    public decimal TotalAmount { get; set; } = 0;
    public CategoryType CategoryType { get; set; } = CategoryType.Expense; // Mostly for Expenses
    public List<OperationCategoryEntity> Allocation { get; set; } = new();
    public int UserId { get; set; }
    public bool IsEnabled { get; set; } = true;
}