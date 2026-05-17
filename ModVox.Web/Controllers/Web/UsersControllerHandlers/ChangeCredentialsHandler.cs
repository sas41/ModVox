using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Endpoints.UsersControllerHandlers;

public sealed class ChangeCredentialsHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;

    public ChangeCredentialsHandler(
        IAccountAuthorizationService authorizationService,
        IUserRepository userRepository,
        IPasswordService passwordService)
    {
        _authorizationService = authorizationService;
        _userRepository = userRepository;
        _passwordService = passwordService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, ChangeCredentialsRequest request, CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        if (user.IsBanned(DateTimeOffset.UtcNow))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        if (string.IsNullOrWhiteSpace(request.NewUsername) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return Results.BadRequest(new { message = "New username and password are required." });
        }

        var newUsername = request.NewUsername.Trim();
        if (newUsername.Contains(' '))
        {
            return Results.BadRequest(new { message = "Username cannot contain spaces." });
        }

        if (string.Equals(newUsername, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(new { message = "Username 'admin' is forbidden." });
        }

        var usernameOwner = await _userRepository.GetByUsernameAsync(newUsername, cancellationToken);
        if (usernameOwner is not null && usernameOwner.Id != user.Id)
        {
            return Results.Conflict(new { message = "Username already exists." });
        }

        if (request.NewPassword.Length < 8)
        {
            return Results.BadRequest(new { message = "Password must be at least 8 characters." });
        }

        var updated = user with
        {
            Username = newUsername,
            PasswordHash = _passwordService.Hash(request.NewPassword),
            MustChangeCredentials = false,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.UpdateAsync(updated, cancellationToken);
        return Results.Ok(new ChangeCredentialsResponse(updated.Id, updated.Username, updated.MustChangeCredentials));
    }

    public sealed class ChangeCredentialsRequest
    {
        public string NewUsername { get; init; } = string.Empty;
        public string NewPassword { get; init; } = string.Empty;
    }
}

public sealed record ChangeCredentialsResponse(Guid UserId, string Username, bool MustChangeCredentials);
