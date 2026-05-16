using ModVox.Web.Domain;
using ModVox.Web.Repositories;

namespace ModVox.Web.Security;

public sealed class AccountAuthorizationService : IAccountAuthorizationService
{
    private readonly IAccountSessionService _accountSessionService;

    public AccountAuthorizationService(IAccountSessionService accountSessionService)
    {
        _accountSessionService = accountSessionService;
    }

    public async Task<UserAccount?> GetCurrentUserAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        return await _accountSessionService.GetCurrentUserAsync(httpContext, cancellationToken);
    }

    public bool HasRole(UserAccount user, params string[] allowedRoles)
    {
        foreach (var allowedRole in allowedRoles)
        {
            if (string.Equals(user.Role, allowedRole, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
