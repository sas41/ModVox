using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class UpdateUserUsernameEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admin/users/{userId:guid}/username", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid userId,
        UpdateUserUsernameRequest request,
        IUserRepository userRepository,
        IAccountAuthorizationService authorizationService,
        IAccountSessionService accountSessionService,
        CancellationToken cancellationToken)
    {
        var actor = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (actor is null)
        {
            return Results.Unauthorized();
        }

        if (!authorizationService.HasRole(actor, UserRoles.Admin))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Results.NotFound();
        }

        var username = request.Username.Trim();
        if (string.IsNullOrWhiteSpace(username) || username.Contains(' '))
        {
            return Results.BadRequest(new { message = "Valid username is required." });
        }

        var owner = await userRepository.GetByUsernameAsync(username, cancellationToken);
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

        await userRepository.UpdateAsync(updated, cancellationToken);
        await accountSessionService.LogoutAllAsync(updated, cancellationToken);
        return Results.Ok(new UserAccountResponse(updated.Id, updated.Username, updated.DisplayName, updated.Email, updated.Role, updated.MustChangeCredentials));
    }
}
