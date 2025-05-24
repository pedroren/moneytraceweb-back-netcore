using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Common;

namespace MoneyTrace.Application.Domain
{
  /// <summary>
  /// Category represents the main group of category or concept for transactions.
  /// Contains multiple subcategories.
  /// </summary>
  /// <remarks>Aggregate root for Category-SubCategory</remarks>
  public class CategoryEntity : AuditableEntity
  {
    [Key]
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
  [Owned]
  public class SubCategoryEntity : AuditableEntity
  {
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; } = true;
    public CategoryEntity Category { get; set; }
  }

  public enum CategoryType
  {
    Expense,
    Income
  }
}