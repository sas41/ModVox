using System.Security.Cryptography;
using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class RotateVerifyTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/mods/{modId:guid}/verify-token/rotate", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid modId,
        IModRepository modRepository,
        IAccountAuthorizationService authorizationService,
        CancellationToken cancellationToken)
    {
        var user = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
            return Results.Unauthorized();

        if (user.IsBanned(DateTimeOffset.UtcNow))
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        if (!authorizationService.HasRole(user, UserRoles.Admin, UserRoles.Moderator))
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        var mod = await modRepository.GetByIdAsync(modId, cancellationToken);
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

        await modRepository.UpdateAsync(updated, cancellationToken);

        return Results.Ok(new RotateVerifyTokenResponse(updated.Id, updated.VerifyToken, updated.ModerationStatus));
    }

    private static string GenerateVerifyToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(8);
        return "modvox-" + Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
