using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class CreateUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admin/users", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        CreateUserRequest request,
        IUserRepository userRepository,
        IPasswordService passwordService,
        IAccountAuthorizationService authorizationService,
        CancellationToken cancellationToken)
    {
        var currentUser = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (currentUser is null)
        {
            return Results.Unauthorized();
        }

        if (currentUser.IsBanned(DateTimeOffset.UtcNow) || !authorizationService.HasRole(currentUser, UserRoles.Admin))
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

        var existing = await userRepository.GetByUsernameAsync(username, cancellationToken);
        if (existing is not null)
        {
            return Results.Conflict(new { message = "Username already exists." });
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
            request.Email.Trim(),
            passwordService.Hash(request.Password),
            normalizedRole,
            MustChangeCredentials: true,
            BanType: UserBanTypes.None,
            BanExpiresAt: null,
            SessionVersion: 1,
            IsDeleted: false,
            now,
            now);

        await userRepository.AddAsync(user, cancellationToken);
        return Results.Created($"/api/v1/admin/users/{user.Id}", new CreateUserResponse(user.Id, user.Username, user.DisplayName, user.Email, user.Role, user.MustChangeCredentials));
    }
}
