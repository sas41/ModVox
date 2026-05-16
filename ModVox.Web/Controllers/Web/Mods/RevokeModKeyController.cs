using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class RevokeModKeyEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/mods/{modId:guid}/keys/revoke", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid modId,
        IModRepository modRepository,
        IAccountAuthorizationService authorizationService,
        CancellationToken cancellationToken)
    {
        var user = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var mod = await modRepository.GetByIdAsync(modId, cancellationToken);
        if (mod is null)
        {
            return Results.NotFound();
        }

        var canManage = authorizationService.HasRole(user, UserRoles.Admin) || mod.MaintainerUserId == user.Id;
        if (!canManage)
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var updated = mod with { KeyHash = null, UpdatedAt = DateTimeOffset.UtcNow };
        await modRepository.UpdateAsync(updated, cancellationToken);
        return Results.NoContent();
    }
}
