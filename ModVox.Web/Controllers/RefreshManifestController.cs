using ModVox.Web.Domain;
using ModVox.Web.Manifest;
using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Endpoints;

public sealed class RefreshManifestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/mods/{modId:guid}/manifest/refresh", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        Guid modId,
        RefreshManifestRequest request,
        HttpContext httpContext,
        IModRepository modRepository,
        IAccountAuthorizationService authorizationService,
        IManifestService manifestService,
        CancellationToken cancellationToken)
    {
        var user = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
            return Results.Unauthorized();

        if (user.IsBanned(DateTimeOffset.UtcNow))
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        var mod = await modRepository.GetByIdAsync(modId, cancellationToken);
        if (mod is null)
            return Results.NotFound(new { message = "Mod not found." });

        var isOwner = mod.MaintainerUserId == user.Id;
        var isAdmin = authorizationService.HasRole(user, UserRoles.Admin);
        if (!isOwner && !isAdmin)
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        // Determine which ref to fetch from:
        // explicit override in request → stored default_ref → HEAD
        var fetchRef = !string.IsNullOrWhiteSpace(request?.Ref)
            ? request.Ref.Trim()
            : (!string.IsNullOrWhiteSpace(mod.DefaultRef) ? mod.DefaultRef : "HEAD");

        var manifestResult = await manifestService.ReadAsync(
            mod.Provider, mod.Owner, mod.Repository, fetchRef, cancellationToken);

        switch (manifestResult)
        {
            case ManifestReadResult.NotFound:
                return Results.UnprocessableEntity(new { message = "Manifest file not found in the repository." });

            case ManifestReadResult.Invalid invalid:
                return Results.UnprocessableEntity(new { message = $"Manifest is invalid: {invalid.Reason}" });
        }

        var valid = (ManifestReadResult.Valid)manifestResult;
        var manifest = valid.Manifest;

        // Check verification
        var wasUnverified = string.Equals(
            mod.ModerationStatus, ModModerationStatus.Unverified, StringComparison.OrdinalIgnoreCase);

        var verifyMatched = !string.IsNullOrWhiteSpace(manifest.Verify)
            && string.Equals(manifest.Verify, mod.VerifyToken, StringComparison.Ordinal);

        var newStatus = wasUnverified && verifyMatched
            ? ModModerationStatus.Pending
            : mod.ModerationStatus;

        var updatedMod = mod with
        {
            Name = manifest.Name,
            Description = manifest.Description ?? string.Empty,
            DefaultRef = manifest.DefaultRef,
            ReadmePath = manifest.Readme,
            ChangelogPath = manifest.Changelog,
            ImagesFolder = manifest.Images,
            TagIds = valid.ResolvedTagIds,
            Credits = manifest.Credits,
            ModerationStatus = newStatus,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await modRepository.UpdateAsync(updatedMod, cancellationToken);

        return Results.Ok(new
        {
            mod_id = updatedMod.Id,
            name = updatedMod.Name,
            moderation_status = updatedMod.ModerationStatus,
            verified = verifyMatched && wasUnverified,
            message = verifyMatched && wasUnverified
                ? "Manifest verified. Your mod is now pending moderator approval."
                : wasUnverified
                    ? "Manifest updated. Add the verify token to your manifest and refresh again to verify."
                    : "Manifest updated successfully."
        });
    }
}

public sealed class RefreshManifestRequest
{
    /// <summary>
    /// Optional ref/branch for this refresh only.
    /// If omitted, the mod's stored default_ref is used; falls back to HEAD.
    /// </summary>
    public string? Ref { get; init; }
}
