namespace ModVox.Web.ApiModels;

public sealed record RotateModKeyResponse(
    Guid ModId,
    string NewKey,
    int KeyVersion);
