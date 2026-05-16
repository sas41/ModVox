namespace ModVox.Web.ApiModels;

public sealed record ChangeCredentialsResponse(
    Guid UserId,
    string Username,
    bool MustChangeCredentials);
