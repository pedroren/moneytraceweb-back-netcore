using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

namespace MoneyTrace.Application.Features.Operations;

public record UpdateOperationCommand(int UserId, int OperationId, DateTime Date, string Title, OperationType Type,
    int? VendorId, int AccountId, int? DestinationAccountId, decimal TotalAmount, string Comments,
    (int CategoryId, int SubCategoryId, decimal Ammount)[] Categories) : IRequest<ErrorOr<OperationEntity>>;
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
            .Include(o => o.Categories)
            .ThenInclude(c => c.Category)
            .ThenInclude(c => c.SubCategories)
            .Include(o => o.Categories)
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
        operation.Categories.Clear();

        // Add new categories
        foreach (var category in request.Categories)
        {
            var opCategory = new OperationCategoryEntity
            {
                Category = await _context.Categories.FindAsync(category.CategoryId),
                SubCategory = await _context.SubCategories.FindAsync(category.SubCategoryId),
                Amount = category.Ammount
            };
            operation.Categories.Add(opCategory);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return operation;
    }
}
public sealed class UpdateOperationCommandValidator : AbstractValidator<UpdateOperationCommand>
{
    private readonly AppDbContext _context;
    public UpdateOperationCommandValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User not identified.");
        RuleFor(x => x.OperationId)
            .GreaterThan(0).WithMessage("Operation not identified.");
        RuleFor(x => x.AccountId)
            .GreaterThan(0).WithMessage("Account not identified.");
        RuleFor(x => x.DestinationAccountId)
            .GreaterThan(0).WithMessage("Destination account not identified.");
    }
}