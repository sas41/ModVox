using ModVox.Web.ApiModels;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class UpdateDisplayNameEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/account/change-display-name", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        UpdateDisplayNameRequest request,
        IUserRepository userRepository,
        IAccountAuthorizationService authorizationService,
        CancellationToken cancellationToken)
    {
        var user = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return Results.BadRequest(new { message = "Display name is required." });
        }

        var updated = user with { DisplayName = request.DisplayName.Trim(), UpdatedAt = DateTimeOffset.UtcNow };
        await userRepository.UpdateAsync(updated, cancellationToken);
        return Results.Ok(new { display_name = updated.DisplayName });
    }
}
