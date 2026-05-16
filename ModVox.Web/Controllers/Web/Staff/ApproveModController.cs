using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class ApproveModEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/mods/{modId:guid}/moderation/approve", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid modId,
        IModRepository modRepository,
        IAccountAuthorizationService authorizationService,
        CancellationToken cancellationToken)
    {
        var currentUser = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (currentUser is null)
        {
            return Results.Unauthorized();
        }

        if (currentUser.IsBanned(DateTimeOffset.UtcNow) || !authorizationService.HasRole(currentUser, UserRoles.Admin, UserRoles.Moderator))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var mod = await modRepository.GetByIdAsync(modId, cancellationToken);
        if (mod is null)
        {
            return Results.NotFound(new { message = "Mod not found." });
        }

        var updated = mod with { ModerationStatus = ModModerationStatus.Approved, UpdatedAt = DateTimeOffset.UtcNow };
        await modRepository.UpdateAsync(updated, cancellationToken);
        return Results.Ok(new ModerationActionResponse(updated.Id, updated.ModerationStatus));
    }
}
