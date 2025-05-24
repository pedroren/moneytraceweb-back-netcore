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
        // Hydrate references
        var account = await _context.Accounts.FirstOrDefaultAsync(x => x.UserId == request.UserId && x.Id == request.AccountId, cancellationToken);
        if (account == null)
            return Error.NotFound("Invalid account.");

        AccountEntity? destinationAccount = request.DestinationAccountId.HasValue
            ? await _context.Accounts.FirstOrDefaultAsync(x => x.UserId == request.UserId && x.Id == request.DestinationAccountId.Value, cancellationToken)
            : null;
        if (request.DestinationAccountId.HasValue && destinationAccount == null)
            return Error.NotFound("Invalid destination account.");

        VendorEntity? vendor = request.VendorId.HasValue
            ? await _context.Vendors.FirstOrDefaultAsync(x => x.UserId == request.UserId && x.Id == request.VendorId.Value, cancellationToken)
            : null;
        if (request.VendorId.HasValue && vendor == null)
            return Error.NotFound("Invalid vendor.");

        List<CategoryEntity> categories = new();
        List<SubCategoryEntity> subCategories = new();
        if (request.Allocation != null && request.Allocation.Length > 0)
        {
            var categoriesLookup = await Task.WhenAll(request.Allocation.Select(async c =>
                      await _context.Categories.FirstOrDefaultAsync(x => x.UserId == request.UserId && x.Id == c.CategoryId, cancellationToken)));
            if (categoriesLookup.Any(c => c == null))
                return Error.NotFound("Invalid category.");
            categories = categoriesLookup.Select(c => c).ToList();

            var subCategoriesLookup = await Task.WhenAll(request.Allocation.Select(async c =>
                      await _context.SubCategories.FirstOrDefaultAsync(x => x.Category.UserId == request.UserId && x.Id == c.SubCategoryId, cancellationToken)));
            if (subCategoriesLookup.Any(s => s == null))
                return Error.NotFound("Invalid subcategory.");
            subCategories = subCategoriesLookup.Select(c => c).ToList();
        }

        var operation = new OperationEntity
        {
            UserId = request.UserId,
            Date = request.Date,
            Title = request.Title,
            Type = request.Type,
            VendorId = request.VendorId,
            Vendor = vendor,
            AccountId = request.AccountId,
            Account = account,
            DestinationAccountId = request.DestinationAccountId,
            DestinationAccount = destinationAccount,
            TotalAmount = request.TotalAmount,
            Comments = request.Comments,
            Allocation = request.Allocation.Select((c, idx) => new OperationCategoryEntity
            {
                CategoryId = c.CategoryId,
                Category = categories.FirstOrDefault(x => x.Id == c.CategoryId),
                SubCategoryId = c.SubCategoryId,
                Amount = c.Amount,
                Order = idx
            }).ToList()
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
            .WithMessage("Categories must be unique.")
            .Must((x, categories) => categories.Sum(c => c.Amount) == x.TotalAmount)
            .WithMessage("The sum of the allocations must be equal to the total amount.")
            //CategoryType must be the same for all categories
            .Must((x, categories) =>
            {
                var categoryTypes = categories.Select(c => _context.Categories.Find(c.CategoryId).Type).Distinct();
                return categoryTypes.Count() == 1;
            })
            .WithMessage("All categories must be of the same type.");            
        });

    }

}


