using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class ListGameModsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/games/{gameId:guid}/mods", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid gameId,
        string? q,
        IModRepository modRepository,
        IAccountAuthorizationService authorizationService,
        CancellationToken cancellationToken)
    {
        var user = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        var includeHidden = user is not null && authorizationService.HasRole(user, UserRoles.Admin, UserRoles.Moderator);

        var mods = includeHidden
            ? await modRepository.ListByGameIdAsync(gameId, cancellationToken)
            : await modRepository.ListVisibleByGameIdAsync(gameId, cancellationToken);

        var filtered = string.IsNullOrWhiteSpace(q)
            ? mods
            : mods.Where(x => x.Owner.Contains(q, StringComparison.OrdinalIgnoreCase) || x.Repository.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        var response = filtered
            .OrderByDescending(x => x.DownloadCount)
            .Select(x => new ModListItemResponse(x.Id, x.GameId, x.Provider, x.Owner, x.Repository, x.DownloadCount, x.TagIds, x.ModerationStatus))
            .ToList();

        return Results.Ok(response);
    }
}
