using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

namespace MoneyTrace.Application.Features.Bills;

public record GetBillsQuery(int UserId) : IRequest<ErrorOr<List<BillEntity>>>;
public class GetBillsQueryHandler : IRequestHandler<GetBillsQuery, ErrorOr<List<BillEntity>>>
{
    private readonly AppDbContext _context;

    public GetBillsQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<BillEntity>>> Handle(GetBillsQuery request, CancellationToken cancellationToken)
    {
        var bills = await _context.Bills
            .Where(b => b.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        return bills;
    }
}

public record GetBillByIdQuery(int UserId, int BillId) : IRequest<ErrorOr<BillEntity>>;
public class GetBillByIdQueryHandler : IRequestHandler<GetBillByIdQuery, ErrorOr<BillEntity>>
{
    private readonly AppDbContext _context;

    public GetBillByIdQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<BillEntity>> Handle(GetBillByIdQuery request, CancellationToken cancellationToken)
    {
        var bill = await _context.Bills
            .FirstOrDefaultAsync(b => b.Id == request.BillId && b.UserId == request.UserId, cancellationToken);

        if (bill is null)
        {
            return Error.NotFound("Bill not found.");
        }

        return bill;
    }
}