using ModVox.Web.ApiModels;
using ModVox.Web.Repositories;

namespace ModVox.Web.Endpoints;

public sealed class GetRefreshJobEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/refresh/jobs/{jobId:guid}", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(Guid jobId, IRefreshJobRepository refreshJobRepository, CancellationToken cancellationToken)
    {
        var job = await refreshJobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job is null)
        {
            return Results.NotFound(new { message = "Job not found." });
        }

        var response = new RefreshJobResponse(
            job.Id,
            job.ModId,
            job.Status,
            job.Result,
            job.Error,
            job.EnqueuedAt,
            job.StartedAt,
            job.CompletedAt);

        return Results.Ok(response);
    }
}
