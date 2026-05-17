using ModVox.Web.Caching;
using ModVox.Web.Domain;
using ModVox.Web.Infrastructure.Persistence;
using ModVox.Web.Manifest;
using ModVox.Web.Providers;
using ModVox.Web.Repositories;

namespace ModVox.Web.Services;

public sealed class ContentSyncService : IContentSyncService
{
    private readonly IRepositoryProviderRegistry _providerRegistry;
    private readonly ICacheCoordinator _cacheCoordinator;
    private readonly IMarkdownRenderer _markdownRenderer;
    private readonly IManifestService _manifestService;
    private readonly IModRepository _modRepository;
    private readonly IModReleaseRepository _modReleaseRepository;
    private readonly ModVoxDbContext _dbContext;

    public ContentSyncService(
        IRepositoryProviderRegistry providerRegistry,
        ICacheCoordinator cacheCoordinator,
        IMarkdownRenderer markdownRenderer,
        IManifestService manifestService,
        IModRepository modRepository,
        IModReleaseRepository modReleaseRepository,
        ModVoxDbContext dbContext)
    {
        _providerRegistry = providerRegistry;
        _cacheCoordinator = cacheCoordinator;
        _markdownRenderer = markdownRenderer;
        _manifestService = manifestService;
        _modRepository = modRepository;
        _modReleaseRepository = modReleaseRepository;
        _dbContext = dbContext;
    }

    public async Task<ContentSyncResult> SyncAsync(ModRecord mod, CancellationToken cancellationToken)
    {
        const string manifestStep = "manifest";
        const string verifyStep = "verify";
        const string readmeStep = "readme";
        const string changelogStep = "changelog";
        const string releasesStep = "releases";
        const string persistStep = "persist";

        var manifestResult = await _manifestService.ReadAsync(mod.Provider, mod.Owner, mod.Repository, mod.DefaultRef, cancellationToken);
        if (manifestResult is ManifestReadResult.NotFound)
        {
            return new ContentSyncResult("failed", "Manifest file not found in the repository.", 0, manifestStep, "manifest_not_found");
        }

        if (manifestResult is ManifestReadResult.Invalid invalid)
        {
            return new ContentSyncResult("failed", $"Manifest is invalid: {invalid.Reason}", 0, manifestStep, "manifest_invalid");
        }

        var valid = (ManifestReadResult.Valid)manifestResult;
        var manifest = valid.Manifest;

        if (string.IsNullOrWhiteSpace(manifest.Verify) || !string.Equals(manifest.Verify, mod.VerifyToken, StringComparison.Ordinal))
        {
            return new ContentSyncResult(
                "failed",
                "Verification failed: manifest verify token is missing or does not match this mod. Paste your token into the manifest 'verify' field and commit that change.",
                0,
                verifyStep,
                "verify_mismatch");
        }

        var provider = _providerRegistry.Get(mod.Provider);
        var coordinates = new RepositoryCoordinates(mod.Provider, mod.Owner, mod.Repository, manifest.DefaultRef);

        var readme = await provider.GetFileContentAsync(coordinates, manifest.Readme, cancellationToken);
        if (readme is null)
        {
            return new ContentSyncResult("failed", $"README not found at '{manifest.Readme}' on ref '{manifest.DefaultRef}'.", 0, readmeStep, "readme_not_found");
        }

        var changelog = await provider.GetFileContentAsync(coordinates, manifest.Changelog, cancellationToken);
        if (changelog is null)
        {
            return new ContentSyncResult("failed", $"Changelog not found at '{manifest.Changelog}' on ref '{manifest.DefaultRef}'.", 0, changelogStep, "changelog_not_found");
        }

        var readmeHtml = _markdownRenderer.RenderToSafeHtml(readme);
        var changelogHtml = _markdownRenderer.RenderToSafeHtml(changelog);
        var images = await provider.ListFolderAsync(coordinates, manifest.Images, cancellationToken);
        var imageFiles = images.Where(x => !x.IsDirectory).ToArray();
        var releases = await provider.ListReleasesAsync(coordinates, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var wasUnverified = string.Equals(mod.ModerationStatus, ModModerationStatus.Unverified, StringComparison.OrdinalIgnoreCase);
        var moderationStatus = wasUnverified ? ModModerationStatus.Pending : mod.ModerationStatus;

        var updatedMod = mod with
        {
            Name = manifest.Name,
            Description = manifest.Description ?? string.Empty,
            DefaultRef = manifest.DefaultRef,
            ReadmePath = manifest.Readme,
            ChangelogPath = manifest.Changelog,
            ImagesFolder = manifest.Images,
            ReadmeMarkdown = readme,
            ReadmeHtml = readmeHtml,
            ChangelogMarkdown = changelog,
            ChangelogHtml = changelogHtml,
            ContentFetchedAt = now,
            TagIds = valid.ResolvedTagIds,
            Credits = manifest.Credits,
            ExternalCredits = manifest.ExternalCredits,
            LastAcceptedRefreshAt = now,
            UpdatedAt = now,
            ModerationStatus = moderationStatus
        };

        var releasesUpserted = 0;
        try
        {
            await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await _modRepository.UpdateAsync(updatedMod, cancellationToken);

            var existingByTag = (await _modReleaseRepository.ListByModIdAsync(mod.Id, cancellationToken))
                .ToDictionary(r => r.TagName, r => r, StringComparer.Ordinal);

            foreach (var release in releases)
            {
                existingByTag.TryGetValue(release.TagName, out var existing);
                var releaseId = existing?.Id ?? Guid.NewGuid();
                var mapped = new ModReleaseRecord(
                    releaseId,
                    mod.Id,
                    release.TagName,
                    string.IsNullOrWhiteSpace(release.Name) ? release.TagName : release.Name,
                    release.IsPrerelease,
                    release.PublishedAt,
                    now)
                {
                    IsHidden = existing?.IsHidden ?? false
                };

                foreach (var artifact in release.Artifacts)
                {
                    mapped.Artifacts.Add(new ModReleaseArtifactRecord(
                        Guid.NewGuid(),
                        releaseId,
                        artifact.Name,
                        artifact.ContentType,
                        artifact.Size,
                        artifact.DownloadUrl.ToString()));
                }

                await _modReleaseRepository.UpsertAsync(mapped, cancellationToken);
                releasesUpserted++;
            }

            await tx.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return new ContentSyncResult("failed", $"Failed to persist refreshed content: {ex.Message}", 0, persistStep, "persist_failed");
        }

        try
        {
            await _cacheCoordinator.InvalidateRepositoryAsync(mod.Provider, mod.Owner, mod.Repository, cancellationToken);

            await _cacheCoordinator.SetAsync(CacheResourceType.Readme, mod.Provider, mod.Owner, mod.Repository, updatedMod.DefaultRef, updatedMod.ReadmePath, readmeHtml, false, cancellationToken);
            await _cacheCoordinator.SetAsync(CacheResourceType.Changelog, mod.Provider, mod.Owner, mod.Repository, updatedMod.DefaultRef, updatedMod.ChangelogPath, changelogHtml, false, cancellationToken);
            await _cacheCoordinator.SetAsync(CacheResourceType.Images, mod.Provider, mod.Owner, mod.Repository, updatedMod.DefaultRef, updatedMod.ImagesFolder, imageFiles, false, cancellationToken);
            await _cacheCoordinator.SetAsync(CacheResourceType.Releases, mod.Provider, mod.Owner, mod.Repository, updatedMod.DefaultRef, "releases", releases, false, cancellationToken);
        }
        catch
        {
            // Cache is an acceleration layer; DB writes are authoritative.
        }

        return new ContentSyncResult("updated", null, releasesUpserted, releasesStep, null);
    }
}
