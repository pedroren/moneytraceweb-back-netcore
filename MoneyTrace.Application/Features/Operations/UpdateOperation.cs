using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

namespace MoneyTrace.Application.Features.Operations;

public record UpdateOperationCommand(int UserId, int OperationId, DateTime Date, string Title, OperationType Type,
    int? VendorId, int AccountId, int? DestinationAccountId, decimal TotalAmount, string Comments,
    OperationCategoryModel[] Allocation) : OperationEntityCommand(UserId, Date, Title, Type, VendorId,
    AccountId, DestinationAccountId, TotalAmount, Comments,
    Allocation), IRequest<ErrorOr<OperationEntity>>;
public class UpdateOperationCommandHandler : IRequestHandler<UpdateOperationCommand, ErrorOr<OperationEntity>>
{
    private readonly AppDbContext _context;

    public UpdateOperationCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<OperationEntity>> Handle(UpdateOperationCommand request, CancellationToken cancellationToken)
    {
        var operation = await _context.Operations
            .Include(o => o.Allocation)
            .ThenInclude(c => c.Category)
            .ThenInclude(c => c.SubCategories)
            .Include(o => o.Allocation)
            .ThenInclude(c => c.SubCategory)
            .FirstOrDefaultAsync(o => o.Id == request.OperationId && o.UserId == request.UserId, cancellationToken);

        if (operation == null)
        {
            return Error.NotFound("Operation not found.");
        }

        operation.Date = request.Date;
        operation.Title = request.Title;
        operation.Type = request.Type;
        operation.Vendor = request.VendorId.HasValue ? await _context.Vendors.FindAsync(request.VendorId) : null;
        operation.Account = await _context.Accounts.FindAsync(request.AccountId);
        operation.DestinationAccount = request.DestinationAccountId.HasValue ? await _context.Accounts.FindAsync(request.DestinationAccountId) : null;
        operation.TotalAmount = request.TotalAmount;
        operation.Comments = request.Comments;

        // Clear existing categories
        operation.Allocation.Clear();

        // Add new categories
        foreach (var category in request.Allocation)
        {
            var opCategory = new OperationCategoryEntity
            {
                Category = await _context.Categories.FindAsync(category.CategoryId),
                SubCategory = await _context.SubCategories.FindAsync(category.SubCategoryId),
                Amount = category.Amount
            };
            operation.Allocation.Add(opCategory);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return operation;
    }
}

internal sealed class UpdateOperationCommandValidator : OperationEntityValidator<UpdateOperationCommand>
{
    public UpdateOperationCommandValidator(AppDbContext context) : base(context)
    {
    }
}