//Aggregate root: Operation Entity, aka Transaction
/*
- Type: simple/transfer
- Date: defaults to today 
- Title: option to choose from predefined [[Template]] retrieving included info
- [[Vendor]]
- [[Category]] - [[Sub-category]] (only for simple)
- [[Account]] (source for transfer)
- Destination [[Account]] (only for transfer )
- Amount
- Comments
*/
using Microsoft.EntityFrameworkCore;

namespace MoneyTrace.Application.Domain;

public enum OperationType
{
    Simple,
    Transfer
}

public class OperationEntity
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string Title { get; set; } = string.Empty;
    public OperationType Type { get; set; } = OperationType.Simple;
    public VendorEntity? Vendor { get; set; }
    public AccountEntity Account { get; set; }
    public AccountEntity? DestinationAccount { get; set; }
    public decimal TotalAmount { get; set; } = 0;
    public string Comments { get; set; } = string.Empty;

    public List<OperationCategoryEntity> Categories { get; set; } = new();
    public int UserId { get; set; }
}

// Details of categories for the operation
[Owned]
public class OperationCategoryEntity
{
    public int Id { get; set; }
    public CategoryEntity Category { get; set; }
    public SubCategoryEntity SubCategory { get; set; }
    public decimal Amount { get; set; } = 0;
    public int Order { get; set; }
}