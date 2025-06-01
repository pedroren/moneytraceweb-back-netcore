namespace MoneyTrace.Application.Features.Vendors;

using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

public record UpdateVendorCommand(int UserId, int Id, string Name, bool IsEnabled) : IRequest<ErrorOr<VendorEntity>>;
public class UpdateVendorCommandHandler(AppDbContext context) : IRequestHandler<UpdateVendorCommand, ErrorOr<VendorEntity>>
{
    public async Task<ErrorOr<VendorEntity>> Handle(UpdateVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await context.Vendors
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId, cancellationToken);
        if (vendor == null)
        {
            return Error.NotFound("Vendor not found.");
        }

        vendor.Name = request.Name;
        vendor.IsEnabled = request.IsEnabled;
        await context.SaveChangesAsync(cancellationToken);

        return vendor;
    }
}
internal sealed class UpdateVendorCommandValidator : AbstractValidator<UpdateVendorCommand>
{
    public UpdateVendorCommandValidator(AppDbContext context)
    {
        RuleFor(v => v.UserId)
            .GreaterThan(0)
            .WithMessage("User not identified.");
        RuleFor(v => v.Id)
            .GreaterThan(0)
            .WithMessage("Vendor not identified.");
        RuleFor(v => v.Name)
            .NotEmpty()
            .WithMessage("Vendor name is required.")
            .MaximumLength(100)
            .WithMessage("Vendor name must not exceed 100 characters.")
            .MustAsync(async (x, name, cancellation) =>
            {
                return !await context.Vendors.AnyAsync(v => v.UserId == x.UserId && v.Name == name && v.Id != x.Id, cancellation);
            })
            .WithMessage("Vendor with the same name already exists.");
    }
}