namespace MoneyTrace.RestBackend;

using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MoneyTrace.Application.Features.Operations;
using MoneyTrace.Application.Infraestructure.Services;
using MoneyTrace.RestBackend.Dto;
using MoneyTrace.RestBackend.Security;

public static class OperationEndpoints
{
    public static void MapOperationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/operations").WithTags("Operations");

        group.MapGet("/", GetOperations)
            .WithName("GetOperations");

        group.MapGet("/{id}", GetOperationById)
            .WithName("GetOperationById");

        group.MapPost("/", CreateOperation)
            .WithName("CreateOperation")
            .Accepts<OperationDto>("application/json")
            .Produces<OperationDto>(StatusCodes.Status201Created);

        group.MapPut("/{id}", UpdateOperation)
            .WithName("UpdateOperation")
            .Accepts<OperationDto>("application/json");

        group.MapDelete("/{id}", DeleteOperation)
            .WithName("DeleteOperation")
            .Produces(StatusCodes.Status204NoContent);
    }
    private static async Task<IResult> DeleteOperation(int id, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new DeleteOperationCommand(userId, id));
        return result.Match<IResult>(
          entity => TypedResults.NoContent(),
          errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> UpdateOperation(int id, [FromBody] OperationDto operationDto, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(operationDto.ToUpdateCommand(userId));
        return result.Match<IResult>(
          entity => TypedResults.Ok(entity.ToDto()),
          errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> CreateOperation([FromBody] OperationDto operationDto, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(operationDto.ToCreateCommand(userId));
        return result.Match<IResult>(
          entity => TypedResults.Created($"/api/operations/{entity.Id}", entity.ToDto()),
          errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> GetOperationById(int id, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new GetOperationByIdQuery(userId, id));
        return result.Match<IResult>(
          entity => TypedResults.Ok(entity.ToDto()),
          errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> GetOperations([AsParameters]GetOperationByCriteriaQueryDto criteria, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(criteria.ToGetOperationByCriteriaCommand(userId));
        return result.Match<IResult>(
          entities => TypedResults.Ok(entities.Select(e => e.ToDto())),
          errors => errors.ToTypedResultsError());
    }
}
