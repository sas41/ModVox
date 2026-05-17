using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Endpoints.UsersControllerHandlers;

public sealed class RevokeUserSessionsHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IUserRepository _userRepository;
    private readonly IAccountSessionService _accountSessionService;

    public RevokeUserSessionsHandler(
        IAccountAuthorizationService authorizationService,
        IUserRepository userRepository,
        IAccountSessionService accountSessionService)
    {
        _authorizationService = authorizationService;
        _userRepository = userRepository;
        _accountSessionService = accountSessionService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, Guid userId, CancellationToken cancellationToken)
    {
        var actor = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (actor is null)
        {
            return Results.Unauthorized();
        }

        if (!_authorizationService.HasRole(actor, UserRoles.Admin))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Results.NotFound();
        }

        var updated = user with { SessionVersion = user.SessionVersion + 1, UpdatedAt = DateTimeOffset.UtcNow };
        await _userRepository.UpdateAsync(updated, cancellationToken);
        await _accountSessionService.LogoutAllAsync(updated, cancellationToken);
        return Results.NoContent();
    }
}

public sealed record RevokeUserSessionsResponse(bool NoContent = true);
