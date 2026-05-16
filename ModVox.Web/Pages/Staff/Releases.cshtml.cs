using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Pages.Staff;

public sealed class ReleasesModel : PageModel
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IModReleaseRepository _releaseRepository;
    private readonly IGameRepository _gameRepository;

    public ReleasesModel(
        IAccountAuthorizationService authorizationService,
        IModReleaseRepository releaseRepository,
        IGameRepository gameRepository)
    {
        _authorizationService = authorizationService;
        _releaseRepository = releaseRepository;
        _gameRepository = gameRepository;
    }

    // Filters (bound from query string)
    [BindProperty(SupportsGet = true)] public string? TagFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string? ModFilter { get; set; }
    [BindProperty(SupportsGet = true)] public Guid? GameFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string? PrereleaseFilter { get; set; }  // "yes" | "no" | ""
    [BindProperty(SupportsGet = true)] public string? HiddenFilter { get; set; }      // "yes" | "no" | ""
    [BindProperty(SupportsGet = true)] public int PageNum { get; set; } = 1;

    public const int PageSize = 25;

    public List<ReleaseRow> Releases { get; } = new();
    public int TotalCount { get; private set; }
    public int TotalPages { get; private set; }
    public List<GameOption> Games { get; } = new();
    public bool IsAdmin { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(HttpContext, cancellationToken);
        if (user is null) return Redirect("/login");
        if (!_authorizationService.HasRole(user, UserRoles.Admin, UserRoles.Moderator))
            return StatusCode(StatusCodes.Status403Forbidden);

        IsAdmin = _authorizationService.HasRole(user, UserRoles.Admin);

        var games = await _gameRepository.ListAsync(cancellationToken);
        Games.AddRange(games.Select(g => new GameOption(g.Id, g.Name)));

        var query = new ReleaseSearchQuery
        {
            TagName = string.IsNullOrWhiteSpace(TagFilter) ? null : TagFilter.Trim(),
            ModName = string.IsNullOrWhiteSpace(ModFilter) ? null : ModFilter.Trim(),
            GameId = GameFilter,
            IsPrerelease = PrereleaseFilter == "yes" ? true : PrereleaseFilter == "no" ? false : null,
            IsHidden = HiddenFilter == "yes" ? true : HiddenFilter == "no" ? false : null,
            Page = Math.Max(1, PageNum),
            PageSize = PageSize
        };

        var (items, total) = await _releaseRepository.SearchAsync(query, cancellationToken);
        TotalCount = total;
        TotalPages = (int)Math.Ceiling(total / (double)PageSize);

        foreach (var r in items)
            Releases.Add(new ReleaseRow(r.Id, r.ModId, r.Mod?.Name ?? r.ModId.ToString(),
                r.TagName, r.Name, r.IsPrerelease, r.IsHidden, r.PublishedAt, r.Artifacts.Count));

        return Page();
    }

    public sealed record ReleaseRow(
        Guid ReleaseId, Guid ModId, string ModName,
        string TagName, string Name, bool IsPrerelease,
        bool IsHidden, DateTimeOffset PublishedAt, int ArtifactCount);

    public sealed record GameOption(Guid GameId, string Name);
}
