using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application;
using MoneyTrace.Application.Infraestructure.Persistence;
using MoneyTrace.BlazorApp.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("MoneyTraceDb"));

builder.Services.AddApplication(); // From the application layer
builder.Services.AddAppInfrastructure(builder.Configuration); // From the application layer

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


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
