namespace ModVox.Web.Config;

public sealed class CacheOptions
{
    public const string SectionName = "Cache";

    public int ReadmeTtlMinutes { get; set; } = 30;
    public int ImagesTtlMinutes { get; set; } = 15;
    public int ReleasesTtlMinutes { get; set; } = 10;
    public int ListingTtlMinutes { get; set; } = 5;
    public int PageTtlMinutes { get; set; } = 3;
    public int NegativeTtlMinutes { get; set; } = 2;
    public int StaleWindowMinutes { get; set; } = 5;
}
