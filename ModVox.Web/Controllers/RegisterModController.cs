using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using ModVox.Web.ApiModels;
using ModVox.Web.Config;
using ModVox.Web.Domain;
using ModVox.Web.Manifest;
using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Endpoints;

public sealed class RegisterModEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/mods", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        RegisterModRequest request,
        IModRepository modRepository,
        IGameRepository gameRepository,
        IAccountAuthorizationService authorizationService,
        IModKeyService modKeyService,
        IManifestService manifestService,
        IOptions<ManifestOptions> manifestOptions,
        CancellationToken cancellationToken)
    {
        var user = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
            return Results.Unauthorized();

        if (user.IsBanned(DateTimeOffset.UtcNow))
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        if (!authorizationService.HasRole(user, UserRoles.Maintainer, UserRoles.Admin))
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        if (request.GameId == Guid.Empty)
            return Results.BadRequest(new { message = "game_id is required." });

        var game = await gameRepository.GetByIdAsync(request.GameId, cancellationToken);
        if (game is null)
            return Results.BadRequest(new { message = "Game not found." });

        var hasBlockedMods = await modRepository.HasFlaggedOrHiddenModsForMaintainerAsync(user.Id, cancellationToken);
        if (hasBlockedMods)
            return Results.Json(
                new { message = "You have hidden mods and cannot register new mods until they are resolved." },
                statusCode: StatusCodes.Status403Forbidden);

        if (string.IsNullOrWhiteSpace(request.RepositoryUrl))
            return Results.BadRequest(new { message = "repository_url is required." });

        if (!TryParseRepositoryUrl(request.RepositoryUrl, out var provider, out var owner, out var repo))
            return Results.BadRequest(new { message = "repository_url must be a valid GitHub repository URL (https://github.com/owner/repo)." });

        var existing = await modRepository.GetByCoordinatesAsync(provider, owner, repo, cancellationToken);
        if (existing is not null)
            return Results.Conflict(new { message = "A mod is already registered for that repository." });

        var fetchRef = string.IsNullOrWhiteSpace(request.InitialRef) ? "HEAD" : request.InitialRef.Trim();
        var manifestResult = await manifestService.ReadAsync(provider, owner, repo, fetchRef, cancellationToken);

        switch (manifestResult)
        {
            case ManifestReadResult.NotFound:
                return Results.UnprocessableEntity(new
                {
                    message = $"No {manifestOptions.Value.FileName} found in the repository at ref '{fetchRef}'. " +
                              $"Please add the manifest file before registering."
                });

            case ManifestReadResult.Invalid invalid:
                return Results.UnprocessableEntity(new
                {
                    message = $"Manifest is invalid: {invalid.Reason}"
                });
        }

        var valid = (ManifestReadResult.Valid)manifestResult;
        var manifest = valid.Manifest;

        var verifyToken = GenerateVerifyToken();
        var plaintextKey = modKeyService.GeneratePlaintextKey();
        var keyHash = modKeyService.Hash(plaintextKey);
        var now = DateTimeOffset.UtcNow;

        var mod = new ModRecord(
            Id: Guid.NewGuid(),
            GameId: request.GameId,
            MaintainerUserId: user.Id,
            Provider: provider,
            Owner: owner,
            Repository: repo,
            DefaultRef: manifest.DefaultRef,
            Name: manifest.Name,
            Description: manifest.Description ?? string.Empty,
            ReadmePath: manifest.Readme,
            ChangelogPath: manifest.Changelog,
            ImagesFolder: manifest.Images,
            TagIds: valid.ResolvedTagIds,
            Credits: manifest.Credits,
            DownloadCount: 0,
            ModerationStatus: ModModerationStatus.Unverified,
            VerifyToken: verifyToken,
            KeyHash: keyHash,
            CreatedAt: now,
            UpdatedAt: now,
            LastAcceptedRefreshAt: null,
            KeyVersion: 1);

        await modRepository.AddAsync(mod, cancellationToken);

        return Results.Created($"/api/v1/mods/{mod.Id}", new RegisterModResponse(
            mod.Id,
            mod.GameId,
            mod.MaintainerUserId,
            mod.Provider,
            mod.Owner,
            mod.Repository,
            mod.Name,
            plaintextKey,
            mod.KeyVersion,
            verifyToken,
            manifestOptions.Value.FileName));
    }

    private static string GenerateVerifyToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(8);
        return "modvox-" + Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static bool TryParseRepositoryUrl(
        string url,
        out string provider,
        out string owner,
        out string repository)
    {
        provider = string.Empty;
        owner = string.Empty;
        repository = string.Empty;

        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttps)
            return false;

        var host = uri.Host.ToLowerInvariant();
        var segments = uri.AbsolutePath.Trim('/').Split('/');

        if (host == "github.com" && segments.Length >= 2
            && !string.IsNullOrWhiteSpace(segments[0])
            && !string.IsNullOrWhiteSpace(segments[1]))
        {
            provider = "github";
            owner = segments[0];
            repository = segments[1].TrimEnd('/');
            return true;
        }

        return false;
    }
}
