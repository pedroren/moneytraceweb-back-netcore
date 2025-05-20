namespace MoneyTrace.Application.Features.Vendors;

using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

public record DeleteVendorCommand(int UserId, int Id) : IRequest<ErrorOr<VendorEntity>>;
public class DeleteVendorCommandHandler(AppDbContext context) : IRequestHandler<DeleteVendorCommand, ErrorOr<VendorEntity>>
{
    public async Task<ErrorOr<VendorEntity>> Handle(DeleteVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await context.Vendors
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId, cancellationToken);
        if (vendor == null)
        {
            return Error.NotFound("Vendor not found.");
        }

        context.Vendors.Remove(vendor);
        await context.SaveChangesAsync(cancellationToken);

        return vendor;
    }
}
internal sealed class DeleteVendorCommandValidator : AbstractValidator<DeleteVendorCommand>
{
    public DeleteVendorCommandValidator(AppDbContext context)
    {
        RuleFor(v => v.UserId)
            .GreaterThan(0)
            .WithMessage("User not identified.");
        RuleFor(v => v.Id)
            .GreaterThan(0)
            .WithMessage("Vendor not identified.");
    }
}