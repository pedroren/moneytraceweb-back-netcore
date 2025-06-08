namespace MoneyTrace.Application.Infraestructure.Services;

public interface IUserSecurityService
{
    Task<int> GetUserId();
    Task<bool> IsUserAdmin();
    Task<bool> IsUserAuthenticated();
}