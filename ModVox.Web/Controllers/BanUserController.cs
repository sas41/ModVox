using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class BanUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admin/users/{userId:guid}/ban", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid userId,
        BanUserRequest request,
        IUserRepository userRepository,
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

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Results.NotFound(new { message = "User not found." });
        }

        var requestedType = request.Type.Trim().ToLowerInvariant();
        if (requestedType is not (UserBanTypes.Temporary or UserBanTypes.Permanent))
        {
            return Results.BadRequest(new { message = "type must be 'temporary' or 'permanent'." });
        }

        DateTimeOffset? expiresAt = null;
        if (requestedType == UserBanTypes.Temporary)
        {
            if (!request.DurationMinutes.HasValue || request.DurationMinutes.Value <= 0)
            {
                return Results.BadRequest(new { message = "duration_minutes must be provided for temporary bans." });
            }

            expiresAt = DateTimeOffset.UtcNow.AddMinutes(request.DurationMinutes.Value);
        }

        var updated = user with
        {
            BanType = requestedType,
            BanExpiresAt = expiresAt,
            SessionVersion = user.SessionVersion + 1,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await userRepository.UpdateAsync(updated, cancellationToken);
        return Results.Ok(new BanUserResponse(updated.Id, updated.BanType, updated.BanExpiresAt));
    }
}
