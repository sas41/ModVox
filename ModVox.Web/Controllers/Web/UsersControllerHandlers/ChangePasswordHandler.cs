using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Endpoints.UsersControllerHandlers;

public sealed class ChangePasswordHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IPasswordService _passwordService;
    private readonly IUserRepository _userRepository;
    private readonly IAccountSessionService _accountSessionService;

    public ChangePasswordHandler(
        IAccountAuthorizationService authorizationService,
        IPasswordService passwordService,
        IUserRepository userRepository,
        IAccountSessionService accountSessionService)
    {
        _authorizationService = authorizationService;
        _passwordService = passwordService;
        _userRepository = userRepository;
        _accountSessionService = accountSessionService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        if (!_passwordService.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return Results.BadRequest(new { message = "Current password is invalid." });
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        {
            return Results.BadRequest(new { message = "New password must be at least 8 characters." });
        }

        var updated = user with
        {
            PasswordHash = _passwordService.Hash(request.NewPassword),
            MustChangeCredentials = false,
            SessionVersion = user.SessionVersion + 1,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.UpdateAsync(updated, cancellationToken);
        await _accountSessionService.LogoutAsync(httpContext, cancellationToken);
        return Results.NoContent();
    }

    public sealed class ChangePasswordRequest
    {
        public string CurrentPassword { get; init; } = string.Empty;
        public string NewPassword { get; init; } = string.Empty;
    }
}

public sealed record ChangePasswordResponse(bool NoContent = true);
