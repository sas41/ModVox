using ModVox.Web.Domain;

namespace ModVox.Web.Security;

public interface IAccountSessionService
{
    Task CreateSessionAsync(HttpContext httpContext, UserAccount user, CancellationToken cancellationToken);
    Task<UserAccount?> GetCurrentUserAsync(HttpContext httpContext, CancellationToken cancellationToken);
    Task LogoutAsync(HttpContext httpContext, CancellationToken cancellationToken);
    Task LogoutAllAsync(UserAccount user, CancellationToken cancellationToken);
}
