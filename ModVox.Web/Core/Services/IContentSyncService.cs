using ModVox.Web.Domain;

namespace ModVox.Web.Services;

public interface IContentSyncService
{
    Task<ContentSyncResult> SyncAsync(ModRecord mod, CancellationToken cancellationToken);
}

public sealed record ContentSyncResult(
    string Status,
    string? Message,
    int ReleasesUpserted,
    string? Step,
    string? ErrorCode);
