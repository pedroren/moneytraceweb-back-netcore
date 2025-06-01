namespace MoneyTrace.Application.Features.Templates;

using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Common;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Operations;
using MoneyTrace.Application.Infraestructure.Persistence;

public record DeleteTemplateCommand(int UserId, int TemplateId) : IRequest<ErrorOr<TemplateEntity>>;
public class DeleteTemplateCommandHandler : IRequestHandler<DeleteTemplateCommand, ErrorOr<TemplateEntity>>
{
    private readonly AppDbContext _context;

    public DeleteTemplateCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<TemplateEntity>> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.Templates
            .Include(t => t.Allocation)
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId && t.UserId == request.UserId, cancellationToken);

        if (template == null)
            return Error.NotFound("Template not found.");

        _context.Templates.Remove(template);
        await _context.SaveChangesAsync(cancellationToken);
        return template;
    }
}
       