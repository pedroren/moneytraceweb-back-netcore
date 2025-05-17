using MediatR;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Users;

namespace MoneyTrace.Application.Infraestructure.Persistence
{
  public class AppDbDataSeeder
  {
    private IMediator _mediator;

    public AppDbDataSeeder(IMediator mediator)
    {
      this._mediator = mediator;
    }


    public void SeedData()
    {
      var users = GetUsersTestData();
      var user = users[0];
      var createUserCommand = new CreateUserCommand(user.Name, user.Email, user.PasswordHash);
      var result = _mediator.Send(createUserCommand).Result;
      if (result.IsError)
      {
        throw new Exception("Error creating user " + result.Errors.First().Description);
      }
      //_db.Users.AddRange(users);
      //_db.Accounts.AddRange(GetAccountsTestData(users[0].Id));
      //_db.SaveChanges();
    }

    public List<UserEntity> GetUsersTestData()
    {
      return [
            new UserEntity{
                Id = 1,
                Name = "Admin User",
                Email = "admin@sample.com",
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DateFormat = "yyyy-MM-dd",
                TimeZone = "America/Vancouver",
                PasswordHash = "admin",
                PasswordSalt = "admin"
            }
         ];
    }

    public List<AccountEntity> GetAccountsTestData(int userId)
    {
      return [
            new AccountEntity{
                Name = "CASH",
                UserId = userId,
                Type = AccountType.Debit,
                Balance = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new AccountEntity{
                Name = "Credit Card",
                UserId = userId,
                Type = AccountType.Credit,
                Balance = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new AccountEntity{
                Name = "Savings",
                UserId = userId,
                Type = AccountType.Debit,
                Balance = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
         ];
    }
  }
}