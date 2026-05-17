using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.StaffControllerHandlers;

public sealed class UnbanUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IAccountAuthorizationService _authorizationService;

    public UnbanUserHandler(IUserRepository userRepository, IAccountAuthorizationService authorizationService)
    {
        _userRepository = userRepository;
        _authorizationService = authorizationService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, Guid userId, CancellationToken cancellationToken)
    {
        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (currentUser is null)
        {
            return Results.Unauthorized();
        }

        if (currentUser.IsBanned(DateTimeOffset.UtcNow) || !_authorizationService.HasRole(currentUser, UserRoles.Admin))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Results.NotFound(new { message = "User not found." });
        }

        var updated = user with
        {
            BanType = UserBanTypes.None,
            BanExpiresAt = null,
            SessionVersion = user.SessionVersion + 1,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.UpdateAsync(updated, cancellationToken);
        return Results.Ok(new UnbanUserResponse(updated.Id, updated.BanType));
    }
}

public sealed record UnbanUserResponse(Guid UserId, string BanType);
