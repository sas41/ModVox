using ModVox.Web.Domain;
using ModVox.Web.Repositories;

namespace ModVox.Web.Security;

public sealed class AccountSessionService : IAccountSessionService
{
    public const string CookieName = "__Host-modvox_session";
    private static readonly TimeSpan SessionTtl = TimeSpan.FromHours(8);

    private readonly IAccountSessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;

    public AccountSessionService(IAccountSessionRepository sessionRepository, IUserRepository userRepository)
    {
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
    }

    public async Task CreateSessionAsync(HttpContext httpContext, UserAccount user, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var sessionId = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var session = new AccountSessionRecord(sessionId, user.Id, user.SessionVersion, now, now.Add(SessionTtl), now);
        await _sessionRepository.AddAsync(session, cancellationToken);

        httpContext.Response.Cookies.Append(
            CookieName,
            sessionId,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = session.ExpiresAt.UtcDateTime,
                Path = "/",
                IsEssential = true
            });
    }

    public async Task<UserAccount?> GetCurrentUserAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!httpContext.Request.Cookies.TryGetValue(CookieName, out var sessionId) || string.IsNullOrWhiteSpace(sessionId))
        {
            return null;
        }

        var session = await _sessionRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        if (session.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            await _sessionRepository.DeleteAsync(sessionId, cancellationToken);
            return null;
        }

        var user = await _userRepository.GetByIdAsync(session.UserId, cancellationToken);
        if (user is null || user.IsDeleted)
        {
            await _sessionRepository.DeleteAsync(sessionId, cancellationToken);
            return null;
        }

        if (user.SessionVersion != session.SessionVersion)
        {
            await _sessionRepository.DeleteAsync(sessionId, cancellationToken);
            return null;
        }

        return user;
    }

    public async Task LogoutAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (httpContext.Request.Cookies.TryGetValue(CookieName, out var sessionId) && !string.IsNullOrWhiteSpace(sessionId))
        {
            await _sessionRepository.DeleteAsync(sessionId, cancellationToken);
        }

        httpContext.Response.Cookies.Delete(CookieName, new CookieOptions { Path = "/" });
    }

    public async Task LogoutAllAsync(UserAccount user, CancellationToken cancellationToken)
    {
        await _sessionRepository.DeleteByUserIdAsync(user.Id, cancellationToken);
    }
}
