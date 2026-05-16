using ModVox.Web.Domain;

namespace ModVox.Web.Security;

public interface IAccountAuthorizationService
{
    Task<UserAccount?> GetCurrentUserAsync(HttpContext httpContext, CancellationToken cancellationToken);
    bool HasRole(UserAccount user, params string[] allowedRoles);
}
