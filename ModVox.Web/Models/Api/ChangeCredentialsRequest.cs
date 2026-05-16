namespace ModVox.Web.ApiModels;

public sealed class ChangeCredentialsRequest
{
    public string NewUsername { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}
