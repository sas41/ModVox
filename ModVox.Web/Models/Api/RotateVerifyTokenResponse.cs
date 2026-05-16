namespace ModVox.Web.ApiModels;

public sealed record RotateVerifyTokenResponse(
    Guid ModId,
    string VerifyToken,
    string ModerationStatus);
