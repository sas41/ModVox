using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class CreateTagEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admin/tags", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
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

        if (string.IsNullOrWhiteSpace(request.Label))
        {
            return Results.BadRequest(new { message = "Label is required." });
        }

        var now = DateTimeOffset.UtcNow;
        var tag = new TagRecord(Guid.NewGuid(), request.Label.Trim(), now, now);
        await tagRepository.AddAsync(tag, cancellationToken);
        return Results.Created($"/api/v1/admin/tags/{tag.Id}", new TagResponse(tag.Id, tag.Label));
    }
}
