using ModVox.Web.Domain;

namespace ModVox.Web.Refresh;

public interface IRefreshAcceptanceService
{
    Task<RefreshAcceptanceResult> AcceptAsync(Mod mod, string? idempotencyKey, CancellationToken cancellationToken);
}

public sealed record RefreshAcceptanceResult(
    bool Accepted,
    RefreshJob? Job,
    bool IsDuplicate,
    int? RetryAfterSeconds);
