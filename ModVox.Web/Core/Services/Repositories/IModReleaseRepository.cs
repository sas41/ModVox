using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public interface IModReleaseRepository
{
    Task<IReadOnlyList<ModRelease>> ListByModIdAsync(Guid modId, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<Guid, IReadOnlyList<ModRelease>>> ListByModIdsAsync(IEnumerable<Guid> modIds, CancellationToken cancellationToken);
    Task<ModRelease?> GetByIdAsync(Guid releaseId, CancellationToken cancellationToken);
    Task<ModRelease?> GetByModAndTagAsync(Guid modId, string tagName, CancellationToken cancellationToken);
    Task<(IReadOnlyList<ModRelease> Items, int TotalCount)> SearchAsync(
        ReleaseSearchQuery query, CancellationToken cancellationToken);
    Task UpsertAsync(ModRelease release, CancellationToken cancellationToken);
    Task UpdateAsync(ModRelease release, CancellationToken cancellationToken);
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
