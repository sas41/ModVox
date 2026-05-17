using Microsoft.EntityFrameworkCore;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.StaffControllerHandlers;

public sealed class CreateTagHandler
{
    private readonly ITagRepository _tagRepository;
    private readonly IAccountAuthorizationService _authorizationService;

    public CreateTagHandler(ITagRepository tagRepository, IAccountAuthorizationService authorizationService)
    {
        _tagRepository = tagRepository;
        _authorizationService = authorizationService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, CreateTagRequest request, CancellationToken cancellationToken)
    {
        var actor = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (actor is null)
        {
            return Results.Unauthorized();
        }

        if (!_authorizationService.HasRole(actor, UserRoles.Admin))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        if (string.IsNullOrWhiteSpace(request.Label))
        {
            return Results.BadRequest(new { message = "Label is required." });
        }

        var normalizedLabel = request.Label.Trim();
        var existing = await _tagRepository.GetByLabelAsync(normalizedLabel, cancellationToken);
        if (existing is not null)
        {
            return Results.Conflict(new { message = "Tag label already exists." });
        }

        var now = DateTimeOffset.UtcNow;
        var tag = new Tag(Guid.NewGuid(), normalizedLabel, now, now);
        try
        {
            await _tagRepository.AddAsync(tag, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("ix_tags_label", StringComparison.OrdinalIgnoreCase) == true)
        {
            return Results.Conflict(new { message = "Tag label already exists." });
        }

        return Results.Created($"/api/v1/admin/tags/{tag.Id}", new CreateTagResponse(tag.Id, tag.Label));
    }

    public sealed class CreateTagRequest
    {
        public string Label { get; init; } = string.Empty;
    }
}

public sealed record CreateTagResponse(Guid TagId, string Label);
