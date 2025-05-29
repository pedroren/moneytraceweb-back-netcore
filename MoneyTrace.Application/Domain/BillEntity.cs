using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MoneyTrace.Application.Common;

namespace MoneyTrace.Application.Domain;

/// <summary>
/// Bills Entity
/// Recurrent bills that need to be paid
/// - Requires Template
/// - Payment frequency (monthly, weekly, biweekly, yearly)
/// - Next due date
/// - Last paid date
/// - Last paid amount
/// - Pay Bill action. Can be partial payment, in which case the next due amount is reduced by the paid amount, and NextDueDate isn't affected until due amount reaches 0.
/// - Notes only on Pay Bill action, to be stored in the OperationEntity.
/// </summary>
public class BillEntity : AuditableEntity
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; } 
    public string Name { get; set; } = string.Empty;
    public int TemplateId { get; set; }
    public Frequency PaymentFrequency { get; set; } = Frequency.Monthly;
    public DateTime NextDueDate { get; set; }
    public decimal NextDueAmount { get; set; } = 0;
    public DateTime? LastPaidDate { get; set; }
    public decimal LastPaidAmount { get; set; }
    public bool IsEnabled { get; set; } = true;

    public int PaymentDay { get; set; } // Day of month for monthly, bi-monthly, and yearly; day of week for weekly and biweekly
    public int? PaymentMonth { get; set; } // Month for yearly payments

    // Navigation properties
    [ForeignKey(nameof(TemplateId))]
    public virtual TemplateEntity Template { get; set; }

    //Events?
}

public enum Frequency
{
    Weekly,
    BiWeekly,
    Monthly,
    BiMonthly,
    Yearly
}