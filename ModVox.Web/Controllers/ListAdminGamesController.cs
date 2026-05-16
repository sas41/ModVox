using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class ListAdminGamesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/admin/games", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        string? q,
        int? page,
        int? pageSize,
        IGameRepository gameRepository,
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

        var games = await gameRepository.ListAsync(cancellationToken);
        var filtered = string.IsNullOrWhiteSpace(q)
            ? games
            : games.Where(x =>
                    x.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    x.Slug.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();

        var safePage = Math.Max(1, page ?? 1);
        var safePageSize = Math.Clamp(pageSize ?? 20, 1, 100);
        var totalCount = filtered.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)safePageSize));

        var items = filtered
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(x => new { game_id = x.Id, name = x.Name, slug = x.Slug, is_hidden = x.IsHidden })
            .ToList();

        return Results.Ok(new
        {
            page = safePage,
            page_size = safePageSize,
            total_count = totalCount,
            total_pages = totalPages,
            items
        });
    }
}
