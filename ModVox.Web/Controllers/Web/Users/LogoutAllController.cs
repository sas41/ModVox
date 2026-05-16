using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class LogoutAllEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/logout-all", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        IUserRepository userRepository,
        IAccountAuthorizationService authorizationService,
        IAccountSessionService accountSessionService,
        CancellationToken cancellationToken)
    {
        var user = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var updated = user with { SessionVersion = user.SessionVersion + 1, UpdatedAt = DateTimeOffset.UtcNow };
        await userRepository.UpdateAsync(updated, cancellationToken);
        await accountSessionService.LogoutAllAsync(updated, cancellationToken);
        await accountSessionService.LogoutAsync(httpContext, cancellationToken);
        return Results.NoContent();
    }
}
