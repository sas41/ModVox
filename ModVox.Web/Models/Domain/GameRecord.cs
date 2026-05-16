namespace ModVox.Web.Domain;

public sealed record GameRecord(
    Guid Id,
    string Slug,
    string Name,
    bool IsHidden,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
