using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Endpoints.UsersControllerHandlers;

public sealed class LogoutAllHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IUserRepository _userRepository;
    private readonly IAccountSessionService _accountSessionService;

    public LogoutAllHandler(
        IAccountAuthorizationService authorizationService,
        IUserRepository userRepository,
        IAccountSessionService accountSessionService)
    {
        _authorizationService = authorizationService;
        _userRepository = userRepository;
        _accountSessionService = accountSessionService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var updated = user with { SessionVersion = user.SessionVersion + 1, UpdatedAt = DateTimeOffset.UtcNow };
        await _userRepository.UpdateAsync(updated, cancellationToken);
        await _accountSessionService.LogoutAllAsync(updated, cancellationToken);
        await _accountSessionService.LogoutAsync(httpContext, cancellationToken);
        return Results.NoContent();
    }
}

public sealed record LogoutAllResponse(bool NoContent = true);
