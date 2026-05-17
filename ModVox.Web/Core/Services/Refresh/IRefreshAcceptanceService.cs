using ModVox.Web.Domain;

namespace ModVox.Web.Refresh;

public interface IRefreshAcceptanceService
{
    Task<RefreshAcceptanceResult> AcceptAsync(ModRecord mod, string? idempotencyKey, CancellationToken cancellationToken);
}

public sealed record RefreshAcceptanceResult(
    bool Accepted,
    RefreshJobRecord? Job,
    bool IsDuplicate,
    int? RetryAfterSeconds);
