using ModVox.Web.Repositories;

namespace ModVox.Web.Endpoints.StaffControllerHandlers;

public sealed class ListTagsHandler
{
    private readonly ITagRepository _tagRepository;

    public ListTagsHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<IResult> HandleAsync(CancellationToken cancellationToken)
    {
        var tags = await _tagRepository.ListAsync(cancellationToken);
        var response = tags.Select(x => new ListTagsResponse(x.Id, x.Label)).ToList();
        return Results.Ok(response);
    }
}

public sealed record ListTagsResponse(Guid TagId, string Label);
