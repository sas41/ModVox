namespace ModVox.Web.ApiModels;

public sealed record ModerationActionResponse(
    Guid ModId,
    string ModerationStatus);
