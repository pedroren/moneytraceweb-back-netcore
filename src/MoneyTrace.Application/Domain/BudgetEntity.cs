/*
- Monthly 
- [[Category]]
- Amount
*/
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Common;

namespace MoneyTrace.Application.Domain;

public class BudgetEntity: AuditableEntity
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Frequency Frequency { get; set; } = Frequency.Monthly; //Only Monthly for now
    public List<BudgetCategoryEntity> Categories { get; set; } = new List<BudgetCategoryEntity>();
}

[Owned]
public class BudgetCategoryEntity
{
    [Key]
    public int Id { get; set; }
    public int BudgetId { get; set; }
    public int CategoryId { get; set; }
    public virtual CategoryEntity Category { get; set; } = null!;
    public decimal Amount { get; set; }

    // Navigation properties
    public BudgetEntity Budget { get; set; } = null!;
}