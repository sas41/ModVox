namespace ModVox.Web.ApiModels;

public sealed record RegisterModResponse(
    Guid ModId,
    Guid GameId,
    Guid MaintainerUserId,
    string Provider,
    string Owner,
    string Repository,
    string Name,
    string Key,
    int KeyVersion,
    string VerifyToken,
    string ManifestFileName);
