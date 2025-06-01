namespace MoneyTrace.Application.Features.Templates;

using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Common;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Operations;
using MoneyTrace.Application.Infraestructure.Persistence;

public record UpdateTemplateCommand(int UserId, int Id, string Title, OperationType Type, int? VendorId,
    int AccountId, int? DestinationAccountId, decimal TotalAmount,
    CategoryType? CategoryType, OperationCategoryModel[] Allocation, bool IsEnabled) : IRequest<ErrorOr<TemplateEntity>>;
public class UpdateTemplateCommandHandler : IRequestHandler<UpdateTemplateCommand, ErrorOr<TemplateEntity>> 
{
    private readonly AppDbContext _context;

    public UpdateTemplateCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<TemplateEntity>> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        // Hydrate references
        var account = await _context.Accounts.FirstOrDefaultAsync(x => x.UserId == request.UserId && x.Id == request.AccountId, cancellationToken);
        if (account == null)
            return Error.NotFound("Invalid account.");

        VendorEntity? vendor = request.VendorId.HasValue
            ? await _context.Vendors.FirstOrDefaultAsync(x => x.UserId == request.UserId && x.Id == request.VendorId.Value, cancellationToken)
            : null;
        if (request.VendorId.HasValue && vendor == null)
            return Error.NotFound("Invalid vendor.");

        List<CategoryEntity> categories = new();
        List<SubCategoryEntity> subCategories = new();

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

        var template = await _context.Templates
            .Include(t => t.Allocation)
            .ThenInclude(c => c.Category)
            .Include(t => t.Allocation)
            .ThenInclude(c => c.SubCategory)
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.UserId == request.UserId, cancellationToken);

        if (template == null)
            return Error.NotFound("Template not found.");

        template.IsEnabled = request.IsEnabled;
        template.Title = request.Title;
        template.Type = request.Type;
        template.VendorId = vendor?.Id;
        template.AccountId = account.Id;
        template.DestinationAccountId = request.DestinationAccountId;
        template.TotalAmount = request.TotalAmount;
        template.CategoryType
            = request.CategoryType ?? CategoryType.Expense;
        template.Allocation.Clear();    
        template.Allocation.AddRange(request.Allocation.Select((c, idx) => new OperationCategoryEntity
        {
            CategoryId = c.CategoryId,
            SubCategoryId = c.SubCategoryId,
            Amount = c.Amount,
            Order = idx
        }));
        await _context.SaveChangesAsync(cancellationToken);
        return template;
    }
}
internal sealed class UpdateTemplateCommandValidator : AbstractValidator<UpdateTemplateCommand>
{
    public UpdateTemplateCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(100)
            .WithMessage("Title must be less than 100 characters.");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0)
            .WithMessage("Total amount must be greater than 0.");

        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required.");
    }
}
// RuleFor(x => x.Allocation)