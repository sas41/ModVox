using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;

namespace ModVox.Web.Security;

public sealed class AccountSessionService : IAccountSessionService
{
    private readonly IUserRepository _userRepository;

    public AccountSessionService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task CreateSessionAsync(HttpContext httpContext, UserAccount user, CancellationToken cancellationToken)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("session_version", user.SessionVersion.ToString())
        };

        var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
        var principal = new ClaimsPrincipal(identity);
        await httpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);
    }

    public async Task<UserAccount?> GetCurrentUserAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var userIdValue = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return null;
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null || user.IsDeleted)
        {
            return null;
        }

        var sessionVersionClaim = httpContext.User.FindFirstValue("session_version");
        if (!int.TryParse(sessionVersionClaim, out var sessionVersion) || sessionVersion != user.SessionVersion)
        {
            await httpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return null;
        }

        return user;
    }

    public async Task LogoutAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        await httpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
    }

    public Task LogoutAllAsync(UserAccount user, CancellationToken cancellationToken) => Task.CompletedTask;
}
