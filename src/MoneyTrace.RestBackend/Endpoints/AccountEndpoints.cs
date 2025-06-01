
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MoneyTrace.Application.Features.Accounts;
using MoneyTrace.RestBackend.Dto;
using MoneyTrace.RestBackend.Security;

namespace MoneyTrace.RestBackend;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/accounts").WithTags("Accounts");

        group.MapGet("/", GetAccounts)
            .WithName("GetAccounts");
        group.MapGet("/active", GetAccountsActive)
            .WithName("GetAccountsActive");

        group.MapGet("/{id}", GetAccountById)
            .WithName("GetAccountById");

        group.MapPost("/", CreateAccount)
            .WithName("CreateAccount")
            .Accepts<AccountDto>("application/json")
            .Produces<AccountDto>(StatusCodes.Status201Created);

        group.MapPut("/{id}", UpdateAccount)
            .WithName("UpdateAccount")
            .Accepts<AccountDto>("application/json");

        group.MapDelete("/{id}", DeleteAccount)
            .WithName("DeleteAccount")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> DeleteAccount(int id, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new DeleteAccountCommand(userId, id));
        return result.Match<IResult>(
          entity => TypedResults.NoContent(),
          errors => errors.ToTypedResultsError());
    }

    private static async Task<IResult> UpdateAccount(int id, [FromBody] AccountDto accountDto, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(accountDto.ToUpdateCommand(userId));
        return result.Match<IResult>(
          entity => TypedResults.Ok(entity.ToDto()),
          errors => errors.ToTypedResultsError());
    }

    private static async Task<IResult> CreateAccount([FromBody] AccountDto accountDto, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(accountDto.ToCreateCommand(userId));
        return result.Match<IResult>(
          entity => TypedResults.Created($"/api/accounts/{entity.Id}", entity.ToDto()),
          errors => errors.ToTypedResultsError());
    }

    private static async Task<IResult> GetAccountById(int id, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new GetAccountByIdQuery(userId, id));
        return result.Match<IResult>(
          entity => TypedResults.Ok(entity.ToDto()),
          errors => errors.ToTypedResultsError());
    }

    private static async Task<IResult> GetAccounts(IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var query = new GetUserAccountsQuery(userId);
        var result = await mediator.Send(query);
        return result.Match<IResult>(
          entities => TypedResults.Ok(entities.Select(x => x.ToDto())),
          errors => errors.ToTypedResultsError());
    }

    private static async Task<IResult> GetAccountsActive(IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var query = new GetUserAccountsQuery(userId);
        var result = await mediator.Send(query);
        return result.Match<IResult>(
          entities => TypedResults.Ok(entities.Where(x => x.IsEnabled).Select(x => x.ToDto())),
          errors => errors.ToTypedResultsError());
    }
}
