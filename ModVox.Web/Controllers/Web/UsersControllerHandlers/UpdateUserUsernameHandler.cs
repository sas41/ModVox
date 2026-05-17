using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Endpoints.UsersControllerHandlers;

public sealed class UpdateUserUsernameHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IUserRepository _userRepository;
    private readonly IAccountSessionService _accountSessionService;

    public UpdateUserUsernameHandler(
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
        UpdateUserUsernameRequest request,
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

        var username = request.Username.Trim();
        if (string.IsNullOrWhiteSpace(username) || username.Contains(' '))
        {
            return Results.BadRequest(new { message = "Valid username is required." });
        }

        var owner = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        if (owner is not null && owner.Id != user.Id)
        {
            return Results.Conflict(new { message = "Username already exists." });
        }

        var updated = user with
        {
            Username = username,
            SessionVersion = user.SessionVersion + 1,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.UpdateAsync(updated, cancellationToken);
        await _accountSessionService.LogoutAllAsync(updated, cancellationToken);
        return Results.Ok(new UpdateUserUsernameResponse(updated.Id, updated.Username, updated.DisplayName, updated.Email, updated.Role, updated.MustChangeCredentials));
    }

    public sealed class UpdateUserUsernameRequest
    {
        public string Username { get; init; } = string.Empty;
    }
}

public sealed record UpdateUserUsernameResponse(
    Guid UserId,
    string Username,
    string DisplayName,
    string Email,
    string Role,
    bool MustChangeCredentials);
