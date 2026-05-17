using Microsoft.EntityFrameworkCore;
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

        var normalizedLabel = request.Label.Trim();
        var existing = await tagRepository.GetByLabelAsync(normalizedLabel, cancellationToken);
        if (existing is not null)
        {
            return Results.Conflict(new { message = "Tag label already exists." });
        }

        var now = DateTimeOffset.UtcNow;
        var tag = new TagRecord(Guid.NewGuid(), normalizedLabel, now, now);
        try
        {
            await tagRepository.AddAsync(tag, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("ix_tags_label", StringComparison.OrdinalIgnoreCase) == true)
        {
            return Results.Conflict(new { message = "Tag label already exists." });
        }

        return Results.Created($"/api/v1/admin/tags/{tag.Id}", new TagResponse(tag.Id, tag.Label));
    }
}
