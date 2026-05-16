namespace ModVox.Web.ApiModels;

public sealed record LoginResponse(
    Guid UserId,
    string Username,
    string DisplayName,
    string Role,
    bool IsAdmin,
    bool MustChangeCredentials);
