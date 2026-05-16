using ModVox.Web.ApiModels;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class ChangeCredentialsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/change-credentials", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        ChangeCredentialsRequest request,
        IUserRepository userRepository,
        IPasswordService passwordService,
        IAccountAuthorizationService authorizationService,
        CancellationToken cancellationToken)
    {
        var user = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
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

        var usernameOwner = await userRepository.GetByUsernameAsync(newUsername, cancellationToken);
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
            PasswordHash = passwordService.Hash(request.NewPassword),
            MustChangeCredentials = false,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await userRepository.UpdateAsync(updated, cancellationToken);
        return Results.Ok(new ChangeCredentialsResponse(updated.Id, updated.Username, updated.MustChangeCredentials));
    }
}
