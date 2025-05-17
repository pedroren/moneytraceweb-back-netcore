using System.Net;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application;
using MoneyTrace.Application.Infraestructure.Persistence;
using MoneyTrace.RestBackend;
using MoneyTrace.RestBackend.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAppDbContext, AppDbContext>();
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("MoneyTraceDb"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "MoneyTraceRestAPI";
    config.Title = "MoneyTraceRestAPI v1";
    config.Version = "v1";
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddApplication(); // From the application layer
builder.Services.AddAppInfrastructure(builder.Configuration); // From the application layer

builder.Services.AddTransient<IUserSecurityService, UserSecurityService>(); //Web authentication helper

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "MoneyTraceRestAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });

    // Manually authenticate Admin user for development
    app.Use(async (context, next) =>
    {
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "Admin"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "admin@sample.com"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Administrator"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1")
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "Development");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        context.User = principal;
        await next();
        });
    }
    //

app.UseExceptionHandler(appError => {
    appError.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";
        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            if (contextFeature.Error is UnauthorizedAccessException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsJsonAsync(new {
                    StatusCode = context.Response.StatusCode,
                    Message = contextFeature.Error.Message
                });
            }
            else
            {
                Console.WriteLine(contextFeature.Error);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsJsonAsync(new {
                    StatusCode = context.Response.StatusCode,
                    Message = "Internal Server Error. Please try again later."
                });
            }
        }
    });
});

app.MapUserEndpoints();
app.MapAccountEndpoints();

// Seed data for InMemory database
using(var scope = app.Services.CreateScope())
  {
      var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
      new AppDbDataSeeder(mediator).SeedData();
  }

app.Run();
