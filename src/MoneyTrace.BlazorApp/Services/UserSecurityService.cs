using System.Security.Claims;
using MoneyTrace.Application.Infraestructure.Services;

namespace MoneyTrace.BlazorApp.Services;

public class UserSecurityService : IUserSecurityService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserSecurityService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> IsUserAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

    public async Task<bool> IsUserAdmin()
    {
        var userRoleClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
        return userRoleClaim != null && userRoleClaim.Contains("Administrator");
    }

    public async Task<int> GetUserId()
    {
        if (!await IsUserAuthenticated())
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            throw new UnauthorizedAccessException("User ID claim not found");
        }
        return int.Parse(userIdClaim);
    }
}