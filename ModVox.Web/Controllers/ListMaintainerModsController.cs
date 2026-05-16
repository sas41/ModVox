using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class ListMaintainerModsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/admin/users/{userId:guid}/mods", HandleAsync);
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
        var items = mods.Select(x => new
        {
            mod_id = x.Id,
            game_id = x.GameId,
            name = x.Owner + "/" + x.Repository,
            key_active = !string.IsNullOrWhiteSpace(x.KeyHash)
        }).ToList();

        return Results.Ok(items);
    }
}
