using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class UpdateTagEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admin/tags/{tagId:guid}", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid tagId,
        TagRequest request,
        ITagRepository tagRepository,
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

        var tag = await tagRepository.GetByIdAsync(tagId, cancellationToken);
        if (tag is null)
        {
            return Results.NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Label))
        {
            return Results.BadRequest(new { message = "Label is required." });
        }

        var updated = tag with { Label = request.Label.Trim(), UpdatedAt = DateTimeOffset.UtcNow };
        await tagRepository.UpdateAsync(updated, cancellationToken);
        return Results.Ok(new TagResponse(updated.Id, updated.Label));
    }
}
