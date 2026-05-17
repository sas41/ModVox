using Microsoft.EntityFrameworkCore;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Endpoints.UsersControllerHandlers;

public sealed class CreateUserHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;

    public CreateUserHandler(
        IAccountAuthorizationService authorizationService,
        IUserRepository userRepository,
        IPasswordService passwordService)
    {
        _authorizationService = authorizationService;
        _userRepository = userRepository;
        _passwordService = passwordService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, CreateUserRequest request, CancellationToken cancellationToken)
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

        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Role))
        {
            return Results.BadRequest(new { message = "username, email, password, and role are required." });
        }

        var normalizedRole = request.Role.Trim().ToLowerInvariant();
        if (normalizedRole is not (UserRoles.Admin or UserRoles.Moderator or UserRoles.Maintainer or UserRoles.User))
        {
            return Results.BadRequest(new { message = "Invalid role." });
        }

        var username = request.Username.Trim();
        if (username.Contains(' '))
        {
            return Results.BadRequest(new { message = "Username cannot contain spaces." });
        }

        var existing = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        if (existing is not null)
        {
            return Results.Conflict(new { message = "Username already exists." });
        }

        var email = request.Email.Trim();
        var existingByEmail = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (existingByEmail is not null)
        {
            return Results.Conflict(new { message = "Email already exists." });
        }

        if (request.Password.Length < 8)
        {
            return Results.BadRequest(new { message = "Password must be at least 8 characters." });
        }

        var now = DateTimeOffset.UtcNow;
        var user = new UserAccount(
            Guid.NewGuid(),
            username,
            username,
            email,
            _passwordService.Hash(request.Password),
            normalizedRole,
            MustChangeCredentials: true,
            BanType: UserBanTypes.None,
            BanExpiresAt: null,
            SessionVersion: 1,
            IsDeleted: false,
            now,
            now);

        try
        {
            await _userRepository.AddAsync(user, cancellationToken);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException?.Message.Contains("ix_users_username", StringComparison.OrdinalIgnoreCase) == true ||
            ex.InnerException?.Message.Contains("ix_users_email", StringComparison.OrdinalIgnoreCase) == true)
        {
            return Results.Conflict(new { message = "Username or email already exists." });
        }

        return Results.Created(
            $"/api/v1/admin/users/{user.Id}",
            new CreateUserResponse(user.Id, user.Username, user.DisplayName, user.Email, user.Role, user.MustChangeCredentials));
    }

    public sealed class CreateUserRequest
    {
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
    }
}

public sealed record CreateUserResponse(
    Guid UserId,
    string Username,
    string DisplayName,
    string Email,
    string Role,
    bool MustChangeCredentials);
