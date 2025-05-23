using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;

namespace MoneyTrace.Application.Features.Operations;

public record DeleteOperationCommand(int UserId, int OperationId) : IRequest<ErrorOr<OperationEntity>>;
public class DeleteOperationCommandHandler : IRequestHandler<DeleteOperationCommand, ErrorOr<OperationEntity>>
{
    private readonly AppDbContext _context;

    public DeleteOperationCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<OperationEntity>> Handle(DeleteOperationCommand request, CancellationToken cancellationToken)
    {
        var operation = await _context.Operations
            .Include(o => o.Allocation)
            .ThenInclude(c => c.Category)
            .Include(o => o.Allocation)
            .ThenInclude(c => c.SubCategory)
            .FirstOrDefaultAsync(o => o.Id == request.OperationId && o.UserId == request.UserId, cancellationToken);

        if (operation == null)
        {
            return Error.NotFound("Operation not found.");
        }

        _context.Operations.Remove(operation);
        await _context.SaveChangesAsync(cancellationToken);

        return operation;
    }
}
    public sealed class DeleteOperationCommandValidator : AbstractValidator<DeleteOperationCommand>
    {
        private readonly AppDbContext _context;
        public DeleteOperationCommandValidator(AppDbContext context)
        {
            _context = context;

            RuleFor(x => x.OperationId)
              .GreaterThan(0).WithMessage("Operation not identified.");
            RuleFor(x => x.UserId)
              .GreaterThan(0).WithMessage("User not identified.");
        }
    }
