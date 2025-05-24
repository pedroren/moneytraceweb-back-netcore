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
    OperationCategoryModel[] Allocation
) : OperationEntityCommand(UserId, Date, Title, Type, VendorId,
    AccountId, DestinationAccountId, TotalAmount, Comments,
    Allocation), IRequest<ErrorOr<OperationEntity>>;

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

internal sealed class CreateOperationCommandValidator : OperationEntityValidator<CreateOperationCommand>
{
    public CreateOperationCommandValidator(AppDbContext context) : base(context)
    {
    }
}


