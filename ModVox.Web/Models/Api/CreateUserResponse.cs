namespace ModVox.Web.ApiModels;

public sealed record CreateUserResponse(
    Guid UserId,
    string Username,
    string DisplayName,
    string Email,
    string Role,
    bool MustChangeCredentials);
