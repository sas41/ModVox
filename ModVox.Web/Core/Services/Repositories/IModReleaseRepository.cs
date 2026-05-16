using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public interface IModReleaseRepository
{
    Task<IReadOnlyList<ModReleaseRecord>> ListByModIdAsync(Guid modId, CancellationToken cancellationToken);
    Task<ModReleaseRecord?> GetByIdAsync(Guid releaseId, CancellationToken cancellationToken);
    Task<ModReleaseRecord?> GetByModAndTagAsync(Guid modId, string tagName, CancellationToken cancellationToken);
    Task<(IReadOnlyList<ModReleaseRecord> Items, int TotalCount)> SearchAsync(
        ReleaseSearchQuery query, CancellationToken cancellationToken);
    Task UpsertAsync(ModReleaseRecord release, CancellationToken cancellationToken);
    Task UpdateAsync(ModReleaseRecord release, CancellationToken cancellationToken);
    Task DeleteAsync(Guid releaseId, CancellationToken cancellationToken);
    Task DeleteByModIdAsync(Guid modId, CancellationToken cancellationToken);
}

public sealed class ReleaseSearchQuery
{
    public string? TagName { get; init; }
    public string? ModName { get; init; }
    public Guid? ModId { get; init; }
    public Guid? GameId { get; init; }
    public bool? IsPrerelease { get; init; }
    public bool? IsHidden { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
}
