using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class CreateGameEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admin/games", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        CreateGameRequest request,
        IGameRepository gameRepository,
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

        if (string.IsNullOrWhiteSpace(request.Slug) || string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest(new { message = "slug and name are required." });
        }

        var slug = request.Slug.Trim().ToLowerInvariant();
        var existing = await gameRepository.GetBySlugAsync(slug, cancellationToken);
        if (existing is not null)
        {
            return Results.Conflict(new { message = "Game slug already exists." });
        }

        var now = DateTimeOffset.UtcNow;
        var game = new GameRecord(Guid.NewGuid(), slug, request.Name.Trim(), IsHidden: false, now, now);
        await gameRepository.AddAsync(game, cancellationToken);

        return Results.Created($"/api/v1/games/{game.Id}", new CreateGameResponse(game.Id, game.Slug, game.Name));
    }
}
