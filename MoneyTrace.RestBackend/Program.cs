using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Infraestructure.Persistence;
using MoneyTrace.RestBackend;
using MoneyTrace.RestBackend.Security;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddTransient<IUserSecurityService, UserSecurityService>();

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
    //Manually authenticate Admin user for development

    // Manually authenticate Admin user for development
    app.Use(async (context, next) =>
    {
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "Admin"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "admin@sample.com"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Administrator"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Sid, "1")
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "Development");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        context.User = principal;
        await next();
    });

}

app.MapUserEndpoints();
app.MapAccountEndpoints();

// Seed data for InMemory database
using(var scope = app.Services.CreateScope())
  {
      var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      new AppDbDataSeeder(context).SeedData();
  }

app.Run();
