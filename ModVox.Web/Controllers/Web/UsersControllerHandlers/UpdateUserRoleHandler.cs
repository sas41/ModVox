using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Endpoints.UsersControllerHandlers;

public sealed class UpdateUserRoleHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IUserRepository _userRepository;
    private readonly IAccountSessionService _accountSessionService;

    public UpdateUserRoleHandler(
        IAccountAuthorizationService authorizationService,
        IUserRepository userRepository,
        IAccountSessionService accountSessionService)
    {
        _authorizationService = authorizationService;
        _userRepository = userRepository;
        _accountSessionService = accountSessionService;
    }

    public async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid userId,
        UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
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

        var role = request.Role.Trim().ToLowerInvariant();
        if (role is not (UserRoles.Admin or UserRoles.Moderator or UserRoles.Maintainer or UserRoles.User))
        {
            return Results.BadRequest(new { message = "Invalid role." });
        }

        var updated = user with
        {
            Role = role,
            SessionVersion = user.SessionVersion + 1,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.UpdateAsync(updated, cancellationToken);
        await _accountSessionService.LogoutAllAsync(updated, cancellationToken);
        return Results.Ok(new UpdateUserRoleResponse(updated.Id, updated.Username, updated.DisplayName, updated.Email, updated.Role, updated.MustChangeCredentials));
    }

    public sealed class UpdateUserRoleRequest
    {
        public string Role { get; init; } = string.Empty;
    }
}

public sealed record UpdateUserRoleResponse(
    Guid UserId,
    string Username,
    string DisplayName,
    string Email,
    string Role,
    bool MustChangeCredentials);
