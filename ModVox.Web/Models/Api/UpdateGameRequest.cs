namespace ModVox.Web.ApiModels;

public sealed class UpdateGameRequest
{
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsHidden { get; init; }
}
