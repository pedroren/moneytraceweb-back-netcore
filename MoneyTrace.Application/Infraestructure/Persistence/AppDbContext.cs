using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Common;
using MoneyTrace.Application.Domain;

namespace MoneyTrace.Application.Infraestructure.Persistence;

// public interface IAppDbContext
// {
//   DbSet<UserEntity> Users { get; set; }
//   DbSet<AccountEntity> Accounts { get; set; }
//   DbSet<CategoryEntity> Categories { get; set; }

//   Task<int> SaveChangesAsync(CancellationToken cancellationToken);
// }

public class AppDbContext : DbContext//, IAppDbContext
{
    private readonly IDomainEventService _domainEventService;

    public AppDbContext(DbContextOptions<AppDbContext> options, IDomainEventService domainEventService) : base(options)
    {
        _domainEventService = domainEventService;
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<AccountEntity> Accounts { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<VendorEntity> Vendors { get; set; }

    //Insert sample data when creating the database
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // var dataSeeder = new AppDbDataSeeder();
        // // Seed data for NOT InMemory databases
        // modelBuilder.Entity<UserEntity>().HasData(
        //     dataSeeder.GetUsersTestData()
        // );
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.Now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.Now;
                    break;
                case EntityState.Detached:
                    break;
                case EntityState.Unchanged:
                    break;
                case EntityState.Deleted:
                    break;
                default:
                    break;
            }
        }

        var events = ChangeTracker.Entries<IHasDomainEvent>()
                .Select(x => x.Entity.DomainEvents)
                .SelectMany(x => x)
                .Where(domainEvent => !domainEvent.IsPublished)
                .ToArray();

        var result = await base.SaveChangesAsync(cancellationToken);

        await DispatchEvents(events);

        return result;
    }

    private async Task DispatchEvents(DomainEvent[] events)
    {
        foreach (var @event in events)
        {
            @event.IsPublished = true;
            await _domainEventService.Publish(@event);
        }
    }
}
