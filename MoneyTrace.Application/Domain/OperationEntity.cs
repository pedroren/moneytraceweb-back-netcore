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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Common;

namespace MoneyTrace.Application.Domain;

public enum OperationType
{
    Simple,
    Transfer
}

public class OperationEntity
{
    [Key]
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string Title { get; set; } = string.Empty;
    public OperationType Type { get; set; } = OperationType.Simple;
    public int? VendorId { get; set; }
    [ForeignKey(nameof(VendorId))]
    public VendorEntity? Vendor { get; set; }
    public int AccountId { get; set; }
    [ForeignKey(nameof(AccountId))]
    public AccountEntity Account { get; set; }
    public int? DestinationAccountId { get; set; }
    [ForeignKey(nameof(DestinationAccountId))]
    public AccountEntity? DestinationAccount { get; set; }
    public decimal TotalAmount { get; set; } = 0;
    public string Comments { get; set; } = string.Empty;

    public List<OperationCategoryEntity> Allocation { get; set; } = new();
    public int UserId { get; set; }

    [NotMapped]
    public List<DomainEvent> DomainEvents { get; private set; } = [];
}

// Details of categories for the operation
[Owned]
public class OperationCategoryEntity
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    [ForeignKey(nameof(CategoryId))]
    public CategoryEntity Category { get; set; }
    public int SubCategoryId { get; set; }
    [ForeignKey(nameof(SubCategoryId))]
    public SubCategoryEntity SubCategory { get; set; }
    public decimal Amount { get; set; } = 0;
    public int Order { get; set; }
}

public record OperationCategoryModel(int CategoryId, int SubCategoryId, decimal Amount);