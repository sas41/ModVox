using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ModVox.Web.Pages.Staff;

public sealed class StaffLoginRedirectModel : PageModel
{
    public IActionResult OnGet()
    {
        return Redirect("/login");
    }
}
