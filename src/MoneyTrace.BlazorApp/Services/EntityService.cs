namespace MoneyTrace.BlazorApp.Services;

// Services/EntityService.cs
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Configuration.UserSecrets;

public class EntityService<TEntity, TId>
{
    private readonly IMediator _mediator;

    public EntityService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<ErrorOr<List<TEntity>>> GetAllAsync<TQuery>(int userId)
        where TQuery : IRequest<ErrorOr<List<TEntity>>>
    {
        var query = Activator.CreateInstance(typeof(TQuery), userId);
        if (query == null)
        {
            return Error.Failure("Query.Creation", $"Failed to create instance of {typeof(TQuery).Name}");
        }
        return await _mediator.Send((TQuery)query);
    }

    public async Task<ErrorOr<TEntity>> GetByIdAsync<TQuery>(int userId, TId id)
        where TQuery : IRequest<ErrorOr<TEntity>>
    {
        var query = Activator.CreateInstance(typeof(TQuery), userId, id);
        if (query == null)
        {
            return Error.Failure("Query.Creation", $"Failed to create instance of {typeof(TQuery).Name}");
        }
        return await _mediator.Send((TQuery)query);
    }

    public async Task<ErrorOr<TEntity>> CreateAsync<TCommand>(TCommand command)
        where TCommand : IRequest<ErrorOr<TEntity>>
    {
        return await _mediator.Send(command);
    }

    public async Task<ErrorOr<TEntity>> UpdateAsync<TCommand>(TCommand command)
        where TCommand : IRequest<ErrorOr<TEntity>>
    {
        return await _mediator.Send(command);
    }

    public async Task<ErrorOr<bool>> DeleteAsync<TCommand>(TCommand command)
        where TCommand : IRequest<ErrorOr<bool>>
    {
        return await _mediator.Send(command);
    }

    // Overload for backward compatibility if you need to create delete commands dynamically
    public async Task<ErrorOr<bool>> DeleteAsync<TCommand>(string userId, TId id)
        where TCommand : IRequest<ErrorOr<bool>>
    {
        var command = Activator.CreateInstance(typeof(TCommand), userId, id);
        if (command == null)
        {
            return Error.Failure("Command.Creation", $"Failed to create instance of {typeof(TCommand).Name}");
        }
        return await _mediator.Send((TCommand)command);
    }
}
