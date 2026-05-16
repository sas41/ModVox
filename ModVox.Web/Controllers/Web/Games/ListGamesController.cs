using ModVox.Web.ApiModels;
using ModVox.Web.Repositories;

namespace ModVox.Web.Endpoints;

public sealed class ListGamesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/games", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(string? q, IGameRepository gameRepository, CancellationToken cancellationToken)
    {
        var games = await gameRepository.ListAsync(cancellationToken);
        var filtered = string.IsNullOrWhiteSpace(q)
            ? games
            : games.Where(x => x.Name.Contains(q, StringComparison.OrdinalIgnoreCase) || x.Slug.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        var visible = filtered
            .Where(x => !x.IsHidden)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => new GameListItemResponse(x.Id, x.Name, x.Slug))
            .ToList();

        return Results.Ok(visible);
    }
}
