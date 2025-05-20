namespace MoneyTrace.Application.Features.Vendors;

using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

public record GetVendorsByUserQuery(int UserId) : IRequest<ErrorOr<VendorEntity[]>>;
public class GetVendorsByUserQueryHandler(AppDbContext context) : IRequestHandler<GetVendorsByUserQuery, ErrorOr<VendorEntity[]>>
{
    public async Task<ErrorOr<VendorEntity[]>> Handle(GetVendorsByUserQuery request, CancellationToken cancellationToken)
    {
        var vendors = await context.Vendors.AsNoTracking()
            .Where(x => x.UserId == request.UserId)
            .ToArrayAsync(cancellationToken);

        return vendors;
    }
}
internal sealed class GetVendorsByUserQueryValidator : AbstractValidator<GetVendorsByUserQuery>
{
    public GetVendorsByUserQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User not identified.");
    }
}

public record GetVendorByIdQuery(int UserId, int Id) : IRequest<ErrorOr<VendorEntity>>;
public class GetVendorByIdQueryHandler(AppDbContext context) : IRequestHandler<GetVendorByIdQuery, ErrorOr<VendorEntity>>
{
    public async Task<ErrorOr<VendorEntity>> Handle(GetVendorByIdQuery request, CancellationToken cancellationToken)
    {
        var vendor = await context.Vendors.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId, cancellationToken);

        if (vendor == null)
        {
            return Error.NotFound("Vendor not found.");
        }
        return vendor;
    }
}
internal sealed class GetVendorByIdQueryValidator : AbstractValidator<GetVendorByIdQuery>
{
    public GetVendorByIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User not identified.");
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Vendor not identified.");
    }
}