using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;

namespace MoneyTrace.Application.Infraestructure.Persistence
{
  public interface IAppDbContext
  {
    DbSet<UserEntity> Users { get; set; }
    DbSet<AccountEntity> Accounts { get; set; }
    DbSet<CategoryEntity> Categories { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
  }

  public class AppDbContext : DbContext, IAppDbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<AccountEntity> Accounts { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }

    //Insert sample data when creating the database
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      // var dataSeeder = new AppDbDataSeeder();
      // // Seed data for NOT InMemory databases
      // modelBuilder.Entity<UserEntity>().HasData(
      //     dataSeeder.GetUsersTestData()
      // );
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken){
      return base.SaveChangesAsync(cancellationToken);
    }

  }
}