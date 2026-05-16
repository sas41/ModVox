namespace ModVox.Web.Domain;

public sealed record AccountSessionRecord(
    string SessionId,
    Guid UserId,
    int SessionVersion,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset LastSeenAt);
