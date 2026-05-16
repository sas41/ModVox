namespace ModVox.Web.ApiModels;

public sealed record BanUserResponse(
    Guid UserId,
    string BanType,
    DateTimeOffset? BanExpiresAt);
