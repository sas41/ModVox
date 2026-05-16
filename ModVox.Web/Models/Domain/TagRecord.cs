namespace ModVox.Web.Domain;

public sealed record TagRecord(
    Guid Id,
    string Label,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
