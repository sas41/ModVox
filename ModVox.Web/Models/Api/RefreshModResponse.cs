namespace ModVox.Web.ApiModels;

public sealed record RefreshModResponse(
    Guid ModId,
    Guid JobId,
    string Status,
    DateTimeOffset RefreshedAt,
    int ReleasesUpserted,
    string Message);
