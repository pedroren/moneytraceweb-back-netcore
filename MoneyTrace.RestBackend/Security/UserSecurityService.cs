using MoneyTrace.Application.Infraestructure.Persistence;

namespace MoneyTrace.RestBackend.Security
{
  public interface IUserSecurityService
  {
    Task<int> GetUserId();
    Task<bool> IsUserAdmin();
    Task<bool> IsUserAuthenticated();
  }

  public class UserSecurityService : IUserSecurityService
  {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _dbContext;

    public UserSecurityService(IHttpContextAccessor httpContextAccessor, AppDbContext db)
    {
      _httpContextAccessor = httpContextAccessor;
      _dbContext = db;
    }

    public async Task<bool> IsUserAuthenticated()
    {
      return _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;
    }
    public async Task<bool> IsUserAdmin()
    {
      return _httpContextAccessor.HttpContext.User.IsInRole("Admin");
    }
    public async Task<int> GetUserId()
    {
      if (!await IsUserAuthenticated())
      {
        throw new UnauthorizedAccessException("User is not authenticated");
      }
      var userIdClaim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type.Contains("/sid"))?.Value;

      if (userIdClaim == null)
      {
        throw new UnauthorizedAccessException("User ID claim not found");
      }
      return int.Parse(userIdClaim);
    }
  }
}