namespace ModVox.Web.ApiModels;

public sealed record RefreshModResponse(
    Guid ModId,
    string Status,
    DateTimeOffset RefreshedAt,
    int ReleasesUpserted,
    string Message);
