using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Endpoints.UsersControllerHandlers;

public sealed class UpdateUserPasswordHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IAccountSessionService _accountSessionService;

    public UpdateUserPasswordHandler(
        IAccountAuthorizationService authorizationService,
        IUserRepository userRepository,
        IPasswordService passwordService,
        IAccountSessionService accountSessionService)
    {
        _authorizationService = authorizationService;
        _userRepository = userRepository;
        _passwordService = passwordService;
        _accountSessionService = accountSessionService;
    }

    public async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid userId,
        UpdateUserPasswordRequest request,
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

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        {
            return Results.BadRequest(new { message = "New password must be at least 8 characters." });
        }

        var updated = user with
        {
            PasswordHash = _passwordService.Hash(request.NewPassword),
            MustChangeCredentials = true,
            SessionVersion = user.SessionVersion + 1,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.UpdateAsync(updated, cancellationToken);
        await _accountSessionService.LogoutAllAsync(updated, cancellationToken);
        return Results.Ok(new UpdateUserPasswordResponse(updated.Id, updated.Username, updated.DisplayName, updated.Email, updated.Role, updated.MustChangeCredentials));
    }

    public sealed class UpdateUserPasswordRequest
    {
        public string NewPassword { get; init; } = string.Empty;
    }
}

public sealed record UpdateUserPasswordResponse(
    Guid UserId,
    string Username,
    string DisplayName,
    string Email,
    string Role,
    bool MustChangeCredentials);
