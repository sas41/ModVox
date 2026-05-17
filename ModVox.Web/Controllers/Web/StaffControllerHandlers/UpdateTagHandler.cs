using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.StaffControllerHandlers;

public sealed class UpdateTagHandler
{
    private readonly ITagRepository _tagRepository;
    private readonly IAccountAuthorizationService _authorizationService;

    public UpdateTagHandler(ITagRepository tagRepository, IAccountAuthorizationService authorizationService)
    {
        _tagRepository = tagRepository;
        _authorizationService = authorizationService;
    }

    public async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid tagId,
        UpdateTagRequest request,
        CancellationToken cancellationToken)
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

        var tag = await _tagRepository.GetByIdAsync(tagId, cancellationToken);
        if (tag is null)
        {
            return Results.NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Label))
        {
            return Results.BadRequest(new { message = "Label is required." });
        }

        var updated = tag with { Label = request.Label.Trim(), UpdatedAt = DateTimeOffset.UtcNow };
        await _tagRepository.UpdateAsync(updated, cancellationToken);
        return Results.Ok(new UpdateTagResponse(updated.Id, updated.Label));
    }

    public sealed class UpdateTagRequest
    {
        public string Label { get; init; } = string.Empty;
    }
}

public sealed record UpdateTagResponse(Guid TagId, string Label);
