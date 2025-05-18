using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Users;
using MoneyTrace.Application.Infraestructure.Persistence;
using MoneyTrace.RestBackend.Dto;
using MoneyTrace.RestBackend.Security;

namespace MoneyTrace.RestBackend;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/", async (AppDbContext db) =>
        {
            var records = await db.Users.ToListAsync();
            return TypedResults.Ok(records);
        })
        .WithName("GetAllUsers");

        group.MapGet("/{id}", async Task<IResult> (int id, AppDbContext db) =>
        {
            return await db.Users.FindAsync(id)
            is UserEntity user
                ? TypedResults.Ok(user)
                : TypedResults.NotFound();
        })
        .WithName("GetUserById");

        group.MapGet("/isadmin", async Task<IResult> (IUserSecurityService userSecurityService) =>
        {
            return await userSecurityService.IsUserAdmin()
        is bool isAdmin
          ? TypedResults.Ok(isAdmin)
          : TypedResults.Unauthorized();
        });

        group.MapPost("/", async Task<IResult> (CreateUserCommand userDto, IMediator mediator) =>
        {
            var result = await mediator.Send(userDto);
            return result.Match<IResult>(
      entity => TypedResults.Created($"/api/accounts/{entity.Id}", entity.ToDto()),
      errors => TypedResults.BadRequest(errors));
        })
        .WithName("CreateUser");
    }
}
