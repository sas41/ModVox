using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class GetModByGameEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/games/{gameId:guid}/mods/{modId:guid}", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid gameId,
        Guid modId,
        IModRepository modRepository,
        IAccountAuthorizationService authorizationService,
        CancellationToken cancellationToken)
    {
        var user = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        var includeHidden = user is not null && authorizationService.HasRole(user, UserRoles.Admin, UserRoles.Moderator);

        var mod = await modRepository.GetByGameAndIdAsync(gameId, modId, includeHidden, cancellationToken);
        if (mod is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new ModDetailsResponse(
            mod.Id,
            mod.GameId,
            mod.MaintainerUserId,
            mod.Provider,
            mod.Owner,
            mod.Repository,
            mod.DefaultRef,
            mod.ReadmePath,
            mod.ChangelogPath,
            mod.ImagesFolder,
            mod.TagIds,
            mod.DownloadCount,
            mod.ModerationStatus));
    }
}
