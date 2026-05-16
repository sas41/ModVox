namespace ModVox.Web.ApiModels;

public sealed record UserAccountResponse(
    Guid UserId,
    string Username,
    string DisplayName,
    string Email,
    string Role,
    bool MustChangeCredentials);
