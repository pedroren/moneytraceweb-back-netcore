namespace MoneyTrace.RestBackend;

using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MoneyTrace.Application.Features.Budgets;
using MoneyTrace.Application.Infraestructure.Services;
using MoneyTrace.RestBackend.Dto;
using MoneyTrace.RestBackend.Security;

public static class BudgetEndpoints
{
    public static void MapBudgetEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/budgets").WithTags("Budgets");

        group.MapGet("/", GetBudgets)
            .WithName("GetBudgets");

        group.MapGet("/{id}", GetBudgetById)
            .WithName("GetBudgetById");

        group.MapPost("/", CreateBudget)
            .WithName("CreateBudget")
            .Accepts<BudgetDto>("application/json")
            .Produces<BudgetDto>(StatusCodes.Status201Created);

        group.MapPut("/{id}", UpdateBudget)
            .WithName("UpdateBudget")
            .Accepts<BudgetDto>("application/json");

        group.MapDelete("/{id}", DeleteBudget)
            .WithName("DeleteBudget")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> DeleteBudget(int id, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new DeleteBudgetCommand(userId, id));
        return result.Match<IResult>(
            entity => TypedResults.NoContent(),
            errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> UpdateBudget(int id, [FromBody] BudgetDto budgetDto, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(budgetDto.ToUpdateCommand(id, userId));
        return result.Match<IResult>(
            entity => TypedResults.Ok(entity.ToDto()),
            errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> CreateBudget([FromBody] BudgetDto budgetDto, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(budgetDto.ToCreateCommand(userId));
        return result.Match<IResult>(
            entity => TypedResults.Created($"/api/budgets/{entity.Id}", entity.ToDto()),
            errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> GetBudgetById(int id, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new GetBudgetQuery(userId, id));
        return result.Match<IResult>(
            entity => TypedResults.Ok(entity.ToDto()),
            errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> GetBudgets(IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new GetBudgetsQuery(userId));
        return result.Match<IResult>(
            entities => TypedResults.Ok(entities.Select(e => e.ToDto())),
            errors => errors.ToTypedResultsError());
    }
}