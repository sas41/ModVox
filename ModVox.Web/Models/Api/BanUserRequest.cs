namespace ModVox.Web.ApiModels;

public sealed class BanUserRequest
{
    public string Type { get; init; } = string.Empty;
    public int? DurationMinutes { get; init; }
}
