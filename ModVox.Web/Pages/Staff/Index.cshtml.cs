using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModVox.Web.Domain;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Pages.Staff;

public sealed class StaffIndexModel : PageModel
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IPageIncludeService _pageIncludeService;

    public StaffIndexModel(IAccountAuthorizationService authorizationService, IPageIncludeService pageIncludeService)
    {
        _authorizationService = authorizationService;
        _pageIncludeService = pageIncludeService;
    }

    public bool IsAdmin { get; private set; }
    public string StaffHelpHtml { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(HttpContext, cancellationToken);
        if (user is null)
        {
            return Redirect("/login");
        }

        var isModerator = _authorizationService.HasRole(user, UserRoles.Moderator);
        IsAdmin = _authorizationService.HasRole(user, UserRoles.Admin);
        if (!IsAdmin && !isModerator)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        StaffHelpHtml = await _pageIncludeService.RenderIncludeAsync("staff-help", cancellationToken);
        return Page();
    }
}
