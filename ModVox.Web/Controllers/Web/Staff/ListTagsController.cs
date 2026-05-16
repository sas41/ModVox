using ModVox.Web.ApiModels;
using ModVox.Web.Repositories;

namespace ModVox.Web.Endpoints;

public sealed class ListTagsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/tags", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(ITagRepository tagRepository, CancellationToken cancellationToken)
    {
        var tags = await tagRepository.ListAsync(cancellationToken);
        var response = tags.Select(x => new TagResponse(x.Id, x.Label)).ToList();
        return Results.Ok(response);
    }
}
