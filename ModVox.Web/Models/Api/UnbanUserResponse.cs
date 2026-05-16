namespace ModVox.Web.ApiModels;

public sealed record UnbanUserResponse(
    Guid UserId,
    string BanType);
