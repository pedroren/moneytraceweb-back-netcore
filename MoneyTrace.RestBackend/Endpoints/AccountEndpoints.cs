
using MoneyTrace.RestBackend.Dto;
using MoneyTrace.Application.Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using MoneyTrace.RestBackend.Security;
using MoneyTrace.Application.Features.Accounts;
using MediatR;

namespace MoneyTrace.RestBackend
{
  public static class AccountEndpoints
  {
    public static void MapAccountEndpoints(this IEndpointRouteBuilder routes)
    {
      var group = routes.MapGroup("/api/accounts").WithTags("Accounts");

      group.MapGet("/", GetAccounts)
          .WithName("GetAccounts");

      group.MapGet("/{id}", GetAccountById)
          .WithName("GetAccountById")
          .Produces<AccountDto>(StatusCodes.Status200OK)
          .Produces(StatusCodes.Status404NotFound)
          .Produces(StatusCodes.Status401Unauthorized);

      group.MapPost("/", CreateAccount)
          .WithName("CreateAccount")
          .Accepts<AccountDto>("application/json")
          .Produces<AccountDto>(StatusCodes.Status201Created)
          .Produces(StatusCodes.Status400BadRequest)
          .Produces(StatusCodes.Status401Unauthorized);

      group.MapPut("/{id}", UpdateAccount)
          .WithName("UpdateAccount")
          .Accepts<AccountDto>("application/json")
          .Produces<AccountDto>(StatusCodes.Status200OK)
          .Produces(StatusCodes.Status404NotFound)
          .Produces(StatusCodes.Status400BadRequest)
          .Produces(StatusCodes.Status401Unauthorized);

      group.MapDelete("/{id}", DeleteAccount)
          .WithName("DeleteAccount")
          .Produces(StatusCodes.Status204NoContent)
          .Produces(StatusCodes.Status404NotFound)
          .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<Results<NoContent, NotFound, UnauthorizedHttpResult>> DeleteAccount(int id, HttpContext context, AppDbContext db)
    {
      if (!context.User.Identity.IsAuthenticated)
      {
        return TypedResults.Unauthorized();
      }
      var entity = await db.Accounts.FindAsync(id);
      if (entity == null)
      {
        return TypedResults.NotFound();
      }
      db.Accounts.Remove(entity);
      await db.SaveChangesAsync();
      return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<AccountDto>, NotFound, BadRequest, UnauthorizedHttpResult>>
      UpdateAccount(int id, [FromBody] AccountDto accountDto, HttpContext context, AppDbContext db)
    {
      if (!context.User.Identity.IsAuthenticated)
      {
        return TypedResults.Unauthorized();
      }
      if (accountDto == null || id != accountDto.Id)
      {
        return TypedResults.BadRequest();
      }
      var entity = await db.Accounts.FindAsync(id);
      if (entity == null)
      {
        return TypedResults.NotFound();
      }
      entity.UpdateFromDto(accountDto);

      db.Accounts.Update(entity);
      await db.SaveChangesAsync();

      var resultDto = AccountDto.FromAccountEntity(entity);
      return TypedResults.Ok(resultDto);
    }

    private static async Task<IResult> CreateAccount([FromBody] CreateAccountCommand accountDto, IMediator mediator, IUserSecurityService userSecService)
    {
      var userId = await userSecService.GetUserId();
      if (accountDto == null)
      {
        return TypedResults.BadRequest();
      }
      var result = await mediator.Send(accountDto with { UserId = userId });
      return result.Match<IResult>(
        entity => TypedResults.Created($"/api/accounts/{entity.Id}", AccountDto.FromAccountEntity(entity)),
        errors => TypedResults.BadRequest(errors));
    }

    private static async Task<Results<Ok<AccountDto>, NotFound, UnauthorizedHttpResult>> GetAccountById(int id, AppDbContext db, IUserSecurityService userSecService)
    {
      try
      {
        var userId = await userSecService.GetUserId();
        var entity = await db.Accounts.FindAsync(id);
        if (entity == null)
        {
          return TypedResults.NotFound();
        }
        if (entity.UserId != userId)
        {
          return TypedResults.Unauthorized();
        }
        return TypedResults.Ok(AccountDto.FromAccountEntity(entity));
      }
      catch (UnauthorizedAccessException)
      {
        return TypedResults.Unauthorized();
      }
      
    }

    private static async Task<Results<Ok<List<AccountDto>>, UnauthorizedHttpResult>> GetAccounts(AppDbContext db, IUserSecurityService userSecService)
    {
      var userId = await userSecService.GetUserId();
      var records = await db.Accounts.Where(x => x.UserId == userId).Select(x => AccountDto.FromAccountEntity(x)).ToListAsync();
      return TypedResults.Ok(records);
    }
  }
}