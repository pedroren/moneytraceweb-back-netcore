
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MoneyTrace.Application.Features.Vendors;
using MoneyTrace.RestBackend.Dto;
using MoneyTrace.RestBackend.Security;

namespace MoneyTrace.RestBackend;

public static class VendorEndpoints
{
    public static void MapVendorEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/vendors").WithTags("Vendors");

        group.MapGet("/", GetVendors)
            .WithName("GetVendors");

        group.MapGet("/active", GetVendorsActive)
            .WithName("GetVendorsActive");

        group.MapGet("/{id}", GetVendorById)
            .WithName("GetVendorById");

        group.MapPost("/", CreateVendor)
            .WithName("CreateVendor")
            .Accepts<VendorDto>("application/json")
            .Produces<VendorDto>(StatusCodes.Status201Created);

        group.MapPut("/{id}", UpdateVendor)
            .WithName("UpdateVendor")
            .Accepts<VendorDto>("application/json");

        group.MapDelete("/{id}", DeleteVendor)
            .WithName("DeleteVendor")
            .Produces(StatusCodes.Status204NoContent);
    }

  private static async Task<IResult> GetVendorsActive(IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new GetVendorsByUserQuery(userId));
        return result.Match<IResult>(
          entity => TypedResults.Ok(entity.Where(x => x.IsEnabled).Select(x => x.ToDto())),
          errors => errors.ToTypedResultsError());
    }

  private static async Task<IResult> DeleteVendor(int id, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new DeleteVendorCommand(userId, id));
        return result.Match<IResult>(
          entity => TypedResults.NoContent(),
          errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> UpdateVendor(int id, [FromBody] VendorDto vendorDto, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new UpdateVendorCommand(userId, vendorDto.Id, vendorDto.Name, vendorDto.IsEnabled));
        return result.Match<IResult>(
          entity => TypedResults.Ok(entity.ToDto()),
          errors => errors.ToTypedResultsError());
    }

    private static async Task<IResult> CreateVendor([FromBody] VendorDto vendorDto, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new CreateVendorCommand(userId, vendorDto.Name));
        return result.Match<IResult>(
          entity => TypedResults.Created($"/api/vendors/{entity.Id}", entity.ToDto()),
          errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> GetVendorById(int id, IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new GetVendorByIdQuery(userId, id));
        return result.Match<IResult>(
          entity => TypedResults.Ok(entity.ToDto()),
          errors => errors.ToTypedResultsError());
    }
    private static async Task<IResult> GetVendors(IMediator mediator, IUserSecurityService userSecService)
    {
        var userId = await userSecService.GetUserId();
        var result = await mediator.Send(new GetVendorsByUserQuery(userId));
        return result.Match<IResult>(
          entity => TypedResults.Ok(entity.Select(x => x.ToDto())),
          errors => errors.ToTypedResultsError());
    }
}