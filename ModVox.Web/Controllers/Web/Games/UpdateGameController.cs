using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class UpdateGameEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admin/games/{gameId:guid}", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid gameId,
        UpdateGameRequest request,
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

        var game = await gameRepository.GetByIdAsync(gameId, cancellationToken);
        if (game is null)
        {
            return Results.NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Slug))
        {
            return Results.BadRequest(new { message = "name and slug are required." });
        }

        var slug = request.Slug.Trim().ToLowerInvariant();
        var existing = await gameRepository.GetBySlugAsync(slug, cancellationToken);
        if (existing is not null && existing.Id != game.Id)
        {
            return Results.Conflict(new { message = "Game slug already exists." });
        }

        var updated = game with
        {
            Name = request.Name.Trim(),
            Slug = slug,
            IsHidden = request.IsHidden,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await gameRepository.UpdateAsync(updated, cancellationToken);
        return Results.Ok(new { game_id = updated.Id, name = updated.Name, slug = updated.Slug, is_hidden = updated.IsHidden });
    }
}
