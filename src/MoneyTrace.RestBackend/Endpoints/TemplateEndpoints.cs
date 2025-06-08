
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MoneyTrace.Application.Features.Templates;
using MoneyTrace.Application.Infraestructure.Services;
using MoneyTrace.RestBackend.Dto;
using MoneyTrace.RestBackend.Security;

namespace MoneyTrace.RestBackend;

public static class TemplateEndpoints
{
    public static void MapTemplateEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/templates").WithTags("Templates");

        group.MapGet("/", GetTemplates)
            .WithName("GetTemplates");

        group.MapGet("/active", GetTemplatesActive)
            .WithName("GetTemplatesActive");

        group.MapGet("/{id}", GetTemplateById)
            .WithName("GetTemplateById");

        group.MapPost("/", CreateTemplate)
            .WithName("CreateTemplate")
            .Accepts<TemplateDto>("application/json")
            .Produces<TemplateDto>(StatusCodes.Status201Created);

        group.MapPut("/{id}", UpdateTemplate)
            .WithName("UpdateTemplate")
            .Accepts<TemplateDto>("application/json");

        group.MapDelete("/{id}", DeleteTemplate)
            .WithName("DeleteTemplate")
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/{id}/newoperation", GetNewOperationFromTemplate)
            .WithName("GetNewOperationFromTemplate")
            .Produces<OperationDto>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetTemplates(IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new GetTemplatesQuery(userId));
        return result.Match<IResult>(
          entity => TypedResults.Ok(entity.Select(x => x.ToDto())),
          errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> GetTemplatesActive(IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new GetTemplatesQuery(userId));
        return result.Match<IResult>(
          entity => TypedResults.Ok(entity.Where(x => x.IsEnabled).Select(x => x.ToDto())),
          errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> GetTemplateById(int id, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new GetTemplateByIdQuery(userId, id));
        return result.Match<IResult>(
          entity => TypedResults.Ok(entity),
          errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> CreateTemplate([FromBody] TemplateDto templateDto, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(templateDto.ToCreateCommand(userId));
        return result.Match<IResult>(
          entity => TypedResults.Created($"/api/templates/{entity.Id}", entity.ToDto()),
          errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> UpdateTemplate(int id, [FromBody] TemplateDto templateDto, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(templateDto.ToUpdateCommand(userId));
        return result.Match<IResult>(
          entity => TypedResults.Ok(entity.ToDto()),
          errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> DeleteTemplate(int id, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new DeleteTemplateCommand(userId, id));
        return result.Match<IResult>(
          entity => TypedResults.NoContent(),
          errors => errors.ToTypedResultsError());
    }

    private static async Task<IResult> GetNewOperationFromTemplate(int id, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new GetNewOperationFromTemplateQuery(userId, id));
        return result.Match<IResult>(
          entity => TypedResults.Ok(entity.ToDto()),
          errors => errors.ToTypedResultsError());
    }
}