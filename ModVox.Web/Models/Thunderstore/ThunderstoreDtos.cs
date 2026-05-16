using System.Text.Json.Serialization;

namespace ModVox.Web.ThunderstoreModels;

public sealed record ThunderstoreIndexEntryDto(
    [property: JsonPropertyName("namespace")] string Namespace,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("version_number")] string VersionNumber,
    [property: JsonPropertyName("file_format")] string FileFormat,
    [property: JsonPropertyName("file_size")] long FileSize,
    [property: JsonPropertyName("dependencies")] IReadOnlyList<string> Dependencies);

public sealed record ThunderstorePackageVersionListingDto(
    [property: JsonPropertyName("date_created")] DateTimeOffset DateCreated,
    [property: JsonPropertyName("download_count")] int DownloadCount,
    [property: JsonPropertyName("download_url")] string DownloadUrl,
    [property: JsonPropertyName("install_url")] string InstallUrl,
    [property: JsonPropertyName("version_number")] string VersionNumber);

public sealed record ThunderstorePackageListItemDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("full_name")] string FullName,
    [property: JsonPropertyName("owner")] string Owner,
    [property: JsonPropertyName("package_url")] string PackageUrl,
    [property: JsonPropertyName("donation_link")] string DonationLink,
    [property: JsonPropertyName("date_created")] DateTimeOffset DateCreated,
    [property: JsonPropertyName("date_updated")] DateTimeOffset DateUpdated,
    [property: JsonPropertyName("uuid4")] Guid Uuid4,
    [property: JsonPropertyName("rating_score")] string RatingScore,
    [property: JsonPropertyName("is_pinned")] bool IsPinned,
    [property: JsonPropertyName("is_deprecated")] bool IsDeprecated,
    [property: JsonPropertyName("has_nsfw_content")] bool HasNsfwContent,
    [property: JsonPropertyName("categories")] IReadOnlyList<string> Categories,
    [property: JsonPropertyName("versions")] IReadOnlyList<ThunderstorePackageVersionListingDto> Versions);

public sealed record ThunderstorePackageVersionDto(
    [property: JsonPropertyName("namespace")] string Namespace,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("version_number")] string VersionNumber,
    [property: JsonPropertyName("full_name")] string FullName,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("icon")] string Icon,
    [property: JsonPropertyName("dependencies")] IReadOnlyList<string> Dependencies,
    [property: JsonPropertyName("download_url")] string DownloadUrl,
    [property: JsonPropertyName("downloads")] int Downloads,
    [property: JsonPropertyName("date_created")] DateTimeOffset DateCreated,
    [property: JsonPropertyName("website_url")] string WebsiteUrl,
    [property: JsonPropertyName("is_active")] bool IsActive);

public sealed record ThunderstorePackageDetailDto(
    [property: JsonPropertyName("namespace")] string Namespace,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("full_name")] string FullName,
    [property: JsonPropertyName("owner")] string Owner,
    [property: JsonPropertyName("package_url")] string PackageUrl,
    [property: JsonPropertyName("date_created")] DateTimeOffset DateCreated,
    [property: JsonPropertyName("date_updated")] DateTimeOffset DateUpdated,
    [property: JsonPropertyName("rating_score")] string RatingScore,
    [property: JsonPropertyName("is_pinned")] bool IsPinned,
    [property: JsonPropertyName("is_deprecated")] bool IsDeprecated,
    [property: JsonPropertyName("total_downloads")] string TotalDownloads,
    [property: JsonPropertyName("latest")] ThunderstorePackageVersionDto Latest,
    [property: JsonPropertyName("community_listings")] IReadOnlyList<object> CommunityListings);

public sealed record ThunderstoreMarkdownDto(
    [property: JsonPropertyName("markdown")] string Markdown);
