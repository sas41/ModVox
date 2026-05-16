using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using ModVox.Web.Config;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Pages.Maintainer;

public sealed class EditModModel : PageModel
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IModRepository _modRepository;
    private readonly IGameRepository _gameRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ManifestOptions _manifestOptions;

    public EditModModel(
        IAccountAuthorizationService authorizationService,
        IModRepository modRepository,
        IGameRepository gameRepository,
        ITagRepository tagRepository,
        IOptions<ManifestOptions> manifestOptions)
    {
        _authorizationService = authorizationService;
        _modRepository = modRepository;
        _gameRepository = gameRepository;
        _tagRepository = tagRepository;
        _manifestOptions = manifestOptions.Value;
    }

    [BindProperty(SupportsGet = true)]
    public Guid ModId { get; set; }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Owner { get; private set; } = string.Empty;
    public string Repository { get; private set; } = string.Empty;
    public string GameName { get; private set; } = string.Empty;
    public string ModerationStatus { get; private set; } = string.Empty;
    public string VerifyToken { get; private set; } = string.Empty;
    public string ManifestFileName { get; private set; } = string.Empty;
    public bool IsUnverified { get; private set; }
    public bool KeyActive { get; private set; }
    public long DownloadCount { get; private set; }
    public List<string> TagLabels { get; } = new();
    public List<(Guid UserId, string Text)> Credits { get; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(HttpContext, cancellationToken);
        if (user is null)
            return Redirect("/login");

        if (!_authorizationService.HasRole(user, UserRoles.Maintainer, UserRoles.Admin))
            return StatusCode(StatusCodes.Status403Forbidden);

        var mod = await _modRepository.GetByIdAsync(ModId, cancellationToken);
        if (mod is null)
            return NotFound();

        if (!_authorizationService.HasRole(user, UserRoles.Admin) && mod.MaintainerUserId != user.Id)
            return StatusCode(StatusCodes.Status403Forbidden);

        Name = mod.Name;
        Description = mod.Description;
        Owner = mod.Owner;
        Repository = mod.Repository;
        KeyActive = mod.KeyHash is not null;
        ModerationStatus = mod.ModerationStatus;
        VerifyToken = mod.VerifyToken;
        ManifestFileName = _manifestOptions.FileName;
        IsUnverified = string.Equals(mod.ModerationStatus, ModModerationStatus.Unverified, StringComparison.OrdinalIgnoreCase);
        DownloadCount = mod.DownloadCount;

        var game = await _gameRepository.GetByIdAsync(mod.GameId, cancellationToken);
        GameName = game?.Name ?? mod.GameId.ToString();

        var allTags = await _tagRepository.ListAsync(cancellationToken);
        var tagMap = allTags.ToDictionary(t => t.Id, t => t.Label);
        foreach (var tagId in mod.TagIds)
            if (tagMap.TryGetValue(tagId, out var label))
                TagLabels.Add(label);

        foreach (var kvp in mod.Credits)
            Credits.Add((kvp.Key, kvp.Value));

        return Page();
    }
}
