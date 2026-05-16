namespace ModVox.Web.ApiModels;

public sealed class UpdateUserPasswordRequest
{
    public string NewPassword { get; init; } = string.Empty;
}
