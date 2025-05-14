using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Infraestructure.Persistence;
using MoneyTrace.RestBackend.Security;

namespace MoneyTrace.RestBackend
{
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
    }
  }
}