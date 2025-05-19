namespace MoneyTrace.RestBackend;

using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MoneyTrace.Application.Features.Categories;
using MoneyTrace.RestBackend.Dto;
using MoneyTrace.RestBackend.Security;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", GetCategories)
            .WithName("GetCategories");

        group.MapGet("/{id}", GetCategoryById)
            .WithName("GetCategoryById");

        group.MapPost("/", CreateCategory)
            .WithName("CreateCategory")
            .Accepts<CategoryDto>("application/json")
            .Produces<CategoryDto>(StatusCodes.Status201Created);

        group.MapPut("/{id}", UpdateCategory)
            .WithName("UpdateCategory")
            .Accepts<CategoryDto>("application/json");

        group.MapDelete("/{id}", DeleteCategory)
            .WithName("DeleteCategory")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> DeleteCategory(int id, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new DeleteCategoryCommand(userId, id));
        return result.Match<IResult>(
          entity => TypedResults.NoContent(),
          errors => errors.ToTypedResultsError());
    }


    private static async Task<IResult> UpdateCategory(int id, [FromBody] CategoryDto categoryDto, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(categoryDto.ToUpdateCommand(userId));
        return result.Match<IResult>(
          entity => TypedResults.Ok(entity.ToDto()),
          errors => errors.ToTypedResultsError());
    }


    private static async Task<IResult> CreateCategory([FromBody] CategoryDto categoryDto, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(categoryDto.ToCreateCommand(userId));
        return result.Match<IResult>(
          entity => TypedResults.Created($"/api/categories/{entity.Id}", entity.ToDto()),
          errors => errors.ToTypedResultsError());
    }


    private static async Task<IResult> GetCategoryById(int id, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new GetCategoryByIdQuery(userId, id));
        return result.Match<IResult>(
          entity => TypedResults.Ok(entity.ToDto()),
          errors => errors.ToTypedResultsError());
    }

    private static async Task<IResult> GetCategories(IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new GetUserCategoriesQuery(userId));
        return result.Match<IResult>(
          entities => TypedResults.Ok(entities.Select(e => e.ToDto())),
          errors => errors.ToTypedResultsError());
    }

}
