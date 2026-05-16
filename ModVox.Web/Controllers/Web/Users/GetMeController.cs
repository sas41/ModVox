using ModVox.Web.ApiModels;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class GetMeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/auth/me", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(HttpContext httpContext, IAccountAuthorizationService authorizationService, CancellationToken cancellationToken)
    {
        var user = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new UserAccountResponse(user.Id, user.Username, user.DisplayName, user.Email, user.Role, user.MustChangeCredentials));
    }
}
