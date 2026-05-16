namespace ModVox.Web.Config;

public sealed class RefreshOptions
{
    public const string SectionName = "Refresh";

    public int MinIntervalMinutes { get; set; } = 10;
}
