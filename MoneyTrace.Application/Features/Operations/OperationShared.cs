using ErrorOr;
using FluentValidation;
using MediatR;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

namespace MoneyTrace.Application.Features.Operations;

public record OperationCategoryModel(int CategoryId, int SubCategoryId, decimal Amount);

public abstract record OperationEntityCommand(int UserId, DateTime Date, string Title, OperationType Type, int? VendorId,
    int AccountId, int? DestinationAccountId, decimal TotalAmount, string Comments,
    CategoryType? CategoryType, OperationCategoryModel[] Allocation);

internal abstract class OperationEntityValidator<T> : AbstractValidator<T>
  where T : OperationEntityCommand
{
  private readonly AppDbContext _context;
  public OperationEntityValidator(AppDbContext context)
  {
    _context = context;

    RuleFor(x => x.Title)
      .NotEmpty().WithMessage("Title is required.")
      .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");
    RuleFor(x => x.UserId)
      .GreaterThan(0).WithMessage("User not identified.");
    RuleFor(x => x.AccountId)
      .GreaterThan(0).WithMessage("Account not identified.");
    RuleFor(x => x.DestinationAccountId)
      .GreaterThan(0).When(x => x.Type == OperationType.Transfer).WithMessage("Destination account not identified.");
    RuleFor(x => x.Comments)
    .MaximumLength(500).WithMessage("Comments must not exceed 500 characters.");
    RuleFor(x => x.TotalAmount)
      .NotEqual(0).WithMessage("Total amount must be different than 0.");
    When(x => x.Type == OperationType.Simple, () =>
    {
      RuleFor(x => x.VendorId)
            .GreaterThan(0).WithMessage("Vendor not identified.");
      RuleFor(x => x.Allocation)
          .NotEmpty().WithMessage("At least one category is required.")
          .Must(categories => categories.GroupBy(c => new { c.CategoryId, c.SubCategoryId }).Where(g => g.Count() > 1).Count() == 0)
          .WithMessage("Categories must be unique.")
          .Must((x, categories) => categories.Sum(c => c.Amount) == x.TotalAmount)
          .WithMessage("The sum of the allocations must be equal to the total amount.")
          //CategoryType must be the same for all categories
          .Must((x, categories) => categories.All(c => _context.Categories.Find(c.CategoryId).Type == x.CategoryType))
          .WithMessage("All categories must be of the same type.");
  });

  }

}