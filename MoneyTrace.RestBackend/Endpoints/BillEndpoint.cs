
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MoneyTrace.Application.Features.Bills;
using MoneyTrace.RestBackend.Dto;
using MoneyTrace.RestBackend.Security;

namespace MoneyTrace.RestBackend;

public static class BillEndpoints
{
    public static void MapBillEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/bills").WithTags("Bills");

        group.MapGet("/", GetBills)
            .WithName("GetBills");

        group.MapGet("/active", GetBillsActive)
            .WithName("GetBills");

        group.MapGet("/{id}", GetBillById)
            .WithName("GetBillById");

        group.MapPost("/", CreateBill)
            .WithName("CreateBill")
            .Accepts<BillDto>("application/json")
            .Produces<BillDto>(StatusCodes.Status201Created);

        group.MapPut("/{id}", UpdateBill)
            .WithName("UpdateBill")
            .Accepts<BillDto>("application/json");

        group.MapDelete("/{id}", DeleteBill)
            .WithName("DeleteBill")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> DeleteBill(int id, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new DeleteBillCommand(userId, id));
        return result.Match<IResult>(
            entity => TypedResults.NoContent(),
            errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> UpdateBill(int id, [FromBody] BillDto billDto, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(billDto.ToUpdateCommand(userId, id));
        return result.Match<IResult>(
            entity => TypedResults.Ok(entity.ToDto()),
            errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> CreateBill([FromBody] BillDto billDto, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(billDto.ToCreateCommand(userId));
        return result.Match<IResult>(
            entity => TypedResults.Created($"/api/bills/{entity.Id}", entity.ToDto()),
            errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> GetBillById(int id, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new GetBillByIdQuery(userId, id));
        return result.Match<IResult>(
            entity => TypedResults.Ok(entity.ToDto()),
            errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> GetBills(IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new GetBillsQuery(userId));
        return result.Match<IResult>(
            entities => TypedResults.Ok(entities.Select(e => e.ToDto()).ToList()),
            errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> GetBillsActive(IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new GetBillsQuery(userId));
        return result.Match<IResult>(
            entities => TypedResults.Ok(entities.Where(x => x.IsEnabled).Select(e => e.ToDto()).ToList()),
            errors => errors.ToTypedResultsError());
    }
}