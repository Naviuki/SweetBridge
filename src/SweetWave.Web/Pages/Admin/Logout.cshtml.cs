using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SweetWave.Web.Pages.Admin;

/// <summary>
/// Обработчик выхода администратора.
/// </summary>
[AllowAnonymous]
public sealed class LogoutModel : PageModel
{
    /// <summary>
    /// Удаляет cookie аутентификации.
    /// </summary>
    /// <returns>Возврат на главную страницу.</returns>
    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToPage("/Index");
    }

    /// <summary>
    /// Выйти разрешено только обработчику POST.
    /// </summary>
    /// <returns>Возврат на главную страницу.</returns>
    public IActionResult OnGet() => RedirectToPage("/Index");
}
