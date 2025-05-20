namespace MoneyTrace.Application.Features.Vendors;

using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

public record CreateVendorCommand(int UserId, string Name) : IRequest<ErrorOr<VendorEntity>>;

public class CreateVendorCommandHandler(AppDbContext context) : IRequestHandler<CreateVendorCommand, ErrorOr<VendorEntity>>
{
    public async Task<ErrorOr<VendorEntity>> Handle(CreateVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = new VendorEntity
        {
            Name = request.Name,
            IsEnabled = true,
            UserId = request.UserId
        };

        context.Vendors.Add(vendor);
        await context.SaveChangesAsync(cancellationToken);

        return vendor;
    }
}

internal sealed class CreateVendorCommandValidator : AbstractValidator<CreateVendorCommand>
{
    public CreateVendorCommandValidator(AppDbContext context)
    {
        RuleFor(v => v.UserId)
            .GreaterThan(0)
            .WithMessage("User not identified.");
        RuleFor(v => v.Name)
            .NotEmpty()
            .WithMessage("Vendor name is required.")
            .MaximumLength(100)
            .WithMessage("Vendor name must not exceed 100 characters.")
            .MustAsync(async (x, name, cancellation) =>
            {
                return !await context.Vendors.AnyAsync(v => v.UserId == x.UserId && v.Name == name, cancellation);
            })
            .WithMessage("Vendor with the same name already exists.");
    }
}