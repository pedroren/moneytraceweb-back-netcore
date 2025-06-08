using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application;
using MoneyTrace.Application.Infraestructure.Persistence;
using MoneyTrace.BlazorApp.Services;
using MoneyTrace.BlazorApp.Components;
using MoneyTrace.Application.Infraestructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("MoneyTraceDb"));

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddApplication(); // From the application layer
builder.Services.AddAppInfrastructure(builder.Configuration); // From the application layer
builder.Services.AddScoped(typeof(EntityService<,>));
builder.Services.AddTransient<IUserSecurityService, UserSecurityService>(); //Web authentication helper

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
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

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Seed initial data for InMemory database
using (var scope = app.Services.CreateScope())
{
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var seeder = new AppDbDataSeeder(mediator);
    await seeder.SeedData();
}

app.Run();
