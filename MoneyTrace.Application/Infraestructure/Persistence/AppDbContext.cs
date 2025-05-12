using Microsoft.EntityFrameworkCore;
using MoneyTrace.Application.Domain;

namespace MoneyTrace.Application.Infraestructure.Persistence
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<AccountEntity> Accounts { get; set; }

    //Insert sample data when creating the database
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      var dataSeeder = new AppDbDataSeeder(this);
      // Seed data for NOT InMemory databases
      modelBuilder.Entity<UserEntity>().HasData(
          dataSeeder.GetUsersTestData()
      );
    }

  }
}