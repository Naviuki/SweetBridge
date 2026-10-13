using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SweetWave.Web.Pages.Admin;

/// <summary>
/// Sign out handler of the administrator.
/// </summary>
[AllowAnonymous]
public sealed class LogoutModel : PageModel
{
    /// <summary>
    /// Deletes the authentication cookie.
    /// </summary>
    /// <returns>Redirect to the home page.</returns>
    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToPage("/Index");
    }

    /// <summary>
    /// Only the POST handler is allowed to sign out.
    /// </summary>
    /// <returns>Redirect to the home page.</returns>
    public IActionResult OnGet() => RedirectToPage("/Index");
}