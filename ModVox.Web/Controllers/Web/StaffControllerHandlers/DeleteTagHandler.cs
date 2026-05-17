using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.StaffControllerHandlers;

public sealed class DeleteTagHandler
{
    private readonly ITagRepository _tagRepository;
    private readonly IAccountAuthorizationService _authorizationService;

    public DeleteTagHandler(ITagRepository tagRepository, IAccountAuthorizationService authorizationService)
    {
        _tagRepository = tagRepository;
        _authorizationService = authorizationService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, Guid tagId, CancellationToken cancellationToken)
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

        await _tagRepository.DeleteAsync(tagId, cancellationToken);
        return Results.NoContent();
    }
}

public sealed record DeleteTagResponse(bool NoContent = true);
