using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class RevokeAllUserModKeysEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admin/users/{userId:guid}/mods/keys/revoke-all", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid userId,
        IUserRepository userRepository,
        IModRepository modRepository,
        IAccountAuthorizationService authorizationService,
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

        var maintainer = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (maintainer is null)
        {
            return Results.NotFound();
        }

        var mods = await modRepository.ListByMaintainerUserIdAsync(userId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var count = 0;

        foreach (var mod in mods)
        {
            if (string.IsNullOrWhiteSpace(mod.KeyHash))
            {
                continue;
            }

            var updated = mod with { KeyHash = null, UpdatedAt = now };
            await modRepository.UpdateAsync(updated, cancellationToken);
            count++;
        }

        return Results.Ok(new { revoked_count = count });
    }
}
