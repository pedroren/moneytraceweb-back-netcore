namespace MoneyTrace.Application.Features.Templates;

using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Common;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Operations;
using MoneyTrace.Application.Infraestructure.Persistence;

public record CreateTemplateCommand(int UserId, string Title, OperationType Type, int? VendorId,
    int AccountId, int? DestinationAccountId, decimal TotalAmount,
    CategoryType? CategoryType, OperationCategoryModel[] Allocation) : IRequest<ErrorOr<TemplateEntity>>;

public class CreateTemplateCommandHandler : IRequestHandler<CreateTemplateCommand, ErrorOr<TemplateEntity>>
{
    private readonly AppDbContext _context;

    public CreateTemplateCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<TemplateEntity>> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
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


        var template = new TemplateEntity
        {
            UserId = request.UserId,
            IsEnabled = true,
            Title = request.Title,
            Type = request.Type,
            VendorId = vendor?.Id,
            AccountId = account.Id,
            DestinationAccountId = request.DestinationAccountId,
            TotalAmount = request.TotalAmount,
            CategoryType = request.CategoryType ?? CategoryType.Expense,
            Allocation = request.Allocation.Select((c, idx) => new OperationCategoryEntity
            {
                CategoryId = c.CategoryId,
                SubCategoryId = c.SubCategoryId,
                Amount = c.Amount,
                Order = idx
            }).ToList()
        };

        _context.Templates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);
        return template;
    }
}
internal sealed class CreateTemplateCommandValidator : AbstractValidator<CreateTemplateCommand>
{
    private readonly AppDbContext _context;

    public CreateTemplateCommandValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User not identified.");
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(100)
            .WithMessage("Title must not exceed 100 characters.");
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User not identified.");
        RuleFor(x => x.AccountId)
            .GreaterThan(0)
            .WithMessage("Account not identified.");
        RuleFor(x => x.Allocation)
            .NotEmpty()
            .WithMessage("At least one category is required.")
            .ForEach(x => x.NotEmpty().WithMessage("Category name is required."));
    }
}