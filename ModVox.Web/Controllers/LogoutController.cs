using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/logout", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(HttpContext httpContext, IAccountSessionService accountSessionService, CancellationToken cancellationToken)
    {
        await accountSessionService.LogoutAsync(httpContext, cancellationToken);
        return Results.NoContent();
    }
}
