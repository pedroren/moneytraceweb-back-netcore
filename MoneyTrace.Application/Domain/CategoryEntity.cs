using MoneyTrace.Application.Common;

namespace MoneyTrace.Application.Domain
{
  /// <summary>
  /// Category represents the main group of category or concept for transactions.
  /// Contains multiple subcategories.
  /// </summary>
  public class CategoryEntity : AuditableEntity
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public CategoryType Type { get; set; } = CategoryType.Expense; // Expense or Income
    public int UserId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public ICollection<SubCategoryEntity> SubCategories { get; set; } = [];
  }

  /// <summary>
  /// SubCategory represents a subdivions of a category.
  /// It's Type and User is the same as the parent.
  /// </summary>
  public class SubCategoryEntity : AuditableEntity
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int CategoryId { get; set; } //Parent Category
    public CategoryEntity Category { get; set; } = new();
  }

  public enum CategoryType
  {
    Expense,
    Income
  }
}