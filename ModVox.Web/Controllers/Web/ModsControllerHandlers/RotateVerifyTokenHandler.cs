using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.ModsControllerHandlers;

public sealed class RotateVerifyTokenHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IModRepository _modRepository;

    public RotateVerifyTokenHandler(IAccountAuthorizationService authorizationService, IModRepository modRepository)
    {
        _authorizationService = authorizationService;
        _modRepository = modRepository;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, Guid modId, CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
            return Results.Unauthorized();

        if (user.IsBanned(DateTimeOffset.UtcNow))
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        if (!_authorizationService.HasRole(user, UserRoles.Admin, UserRoles.Moderator))
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        var mod = await _modRepository.GetByIdAsync(modId, cancellationToken);
        if (mod is null)
            return Results.NotFound(new { message = "Mod not found." });

        var isVerifying = string.Equals(mod.ModerationStatus, ModModerationStatus.Unverified, StringComparison.OrdinalIgnoreCase)
            || string.Equals(mod.ModerationStatus, ModModerationStatus.Pending, StringComparison.OrdinalIgnoreCase);
        if (!isVerifying)
            return Results.BadRequest(new { message = "Verify token can only be rotated for mods in unverified or pending status." });

        var updated = mod with
        {
            VerifyToken = GenerateVerifyToken(),
            ModerationStatus = ModModerationStatus.Unverified,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _modRepository.UpdateAsync(updated, cancellationToken);

        return Results.Ok(new RotateVerifyTokenResponse(updated.Id, updated.VerifyToken, updated.ModerationStatus));
    }

    private static string GenerateVerifyToken()
    {
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(8);
        return "modvox-" + Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

public sealed record RotateVerifyTokenResponse(Guid ModId, string VerifyToken, string ModerationStatus);
