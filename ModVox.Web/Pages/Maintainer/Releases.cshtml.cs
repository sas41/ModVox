using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Pages.Maintainer;

public sealed class ReleasesModel : PageModel
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IModRepository _modRepository;
    private readonly IModReleaseRepository _releaseRepository;

    public ReleasesModel(
        IAccountAuthorizationService authorizationService,
        IModRepository modRepository,
        IModReleaseRepository releaseRepository)
    {
        _authorizationService = authorizationService;
        _modRepository = modRepository;
        _releaseRepository = releaseRepository;
    }

    [BindProperty(SupportsGet = true)]
    public Guid ModId { get; set; }

    public string ModName { get; private set; } = string.Empty;
    public bool IsAdmin { get; private set; }
    public List<ReleaseRow> Releases { get; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(HttpContext, cancellationToken);
        if (user is null) return Redirect("/login");

        if (!_authorizationService.HasRole(user, UserRoles.Maintainer, UserRoles.Admin))
            return StatusCode(StatusCodes.Status403Forbidden);

        var mod = await _modRepository.GetByIdAsync(ModId, cancellationToken);
        if (mod is null) return NotFound();

        IsAdmin = _authorizationService.HasRole(user, UserRoles.Admin);
        if (!IsAdmin && mod.MaintainerUserId != user.Id)
            return StatusCode(StatusCodes.Status403Forbidden);

        ModName = mod.Name;

        var releases = await _releaseRepository.ListByModIdAsync(ModId, cancellationToken);
        foreach (var r in releases)
            Releases.Add(new ReleaseRow(r.Id, r.TagName, r.Name, r.IsPrerelease, r.IsHidden, r.PublishedAt, r.Artifacts.Count));

        return Page();
    }

    public sealed record ReleaseRow(
        Guid ReleaseId, string TagName, string Name,
        bool IsPrerelease, bool IsHidden,
        DateTimeOffset PublishedAt, int ArtifactCount);
}
