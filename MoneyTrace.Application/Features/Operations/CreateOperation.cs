using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Common;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

namespace MoneyTrace.Application.Features.Operations;

public record CreateOperationCommand(int UserId, DateTime Date, string Title, OperationType Type, int? VendorId,
    int AccountId, int? DestinationAccountId, decimal TotalAmount, string Comments,
    OperationCategoryModel[] Allocation) : IRequest<ErrorOr<OperationEntity>>;

public class CreateOperationCommandHandler : IRequestHandler<CreateOperationCommand, ErrorOr<OperationEntity>>
{
  private readonly AppDbContext _context;

  public CreateOperationCommandHandler(AppDbContext context)
  {
    _context = context;
  }

  public async Task<ErrorOr<OperationEntity>> Handle(CreateOperationCommand request, CancellationToken cancellationToken)
  {
    var operation = new OperationEntity
    {
      UserId = request.UserId,
      Date = request.Date,
      Title = request.Title,
      Type = request.Type,
      VendorId = request.VendorId.HasValue ? request.VendorId : null,
      AccountId = request.AccountId,
      DestinationAccountId = request.DestinationAccountId.HasValue ? request.DestinationAccountId : null,
      TotalAmount = request.TotalAmount,
      Comments = request.Comments,
      Allocation = (await Task.WhenAll(request.Allocation.Select(async (c, idx) => new OperationCategoryEntity
      {
        CategoryId = c.CategoryId,
        SubCategoryId = c.SubCategoryId,
        Amount = c.Amount,
        Order = idx
      }))).ToList()
    };

    operation.DomainEvents.Add(new OperationCreatedEvent(operation));

    _context.Operations.Add(operation);
    await _context.SaveChangesAsync(cancellationToken);

    return operation;
  }
}

public sealed class CreateOperationCommandValidator : AbstractValidator<CreateOperationCommand>
{
    private readonly AppDbContext _context;
    public CreateOperationCommandValidator(AppDbContext context)
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
            .WithMessage("Categories must be unique.");
        });

    }

}


