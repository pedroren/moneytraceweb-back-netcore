//Aggregate root: Operation Entity, aka Transaction
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

public class OperationEntity: AuditableEntity, IHasDomainEvent
{
    [Key]
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string Title { get; set; } = string.Empty;
    public OperationType Type { get; set; } = OperationType.Simple;
    public int? VendorId { get; set; }
    [ForeignKey(nameof(VendorId))]
    public virtual VendorEntity? Vendor { get; set; }
    public int AccountId { get; set; }
    [ForeignKey(nameof(AccountId))]
    public virtual AccountEntity Account { get; set; }
    public int? DestinationAccountId { get; set; }
    [ForeignKey(nameof(DestinationAccountId))]
    public virtual AccountEntity? DestinationAccount { get; set; }
    public decimal TotalAmount { get; set; } = 0;
    public string Comments { get; set; } = string.Empty;

    public CategoryType? CategoryType { get; set; } // Explicit type of Categories to use in the operation and affect the accounts
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
    public virtual CategoryEntity Category { get; set; }
    public int SubCategoryId { get; set; }
    [ForeignKey(nameof(SubCategoryId))]
    public virtual SubCategoryEntity SubCategory { get; set; }
    public decimal Amount { get; set; } = 0;
    public int Order { get; set; }
}