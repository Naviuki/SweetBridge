using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SweetBridge.Web.Domain.Entities;

namespace SweetBridge.Web.Pages.Account;

/// <summary>
/// Выход из аккаунта.
/// </summary>
/// <param name="signInManager">Менеджер входа Identity.</param>
[AllowAnonymous]
public sealed class LogoutModel(SignInManager<AppUser> signInManager) : PageModel
{
    /// <summary>
    /// Выходит из аккаунта.
    /// </summary>
    /// <returns>Перенаправление на главную страницу.</returns>
    public async Task<IActionResult> OnGetLogoutAsync()
    {
        await signInManager.SignOutAsync();

        return RedirectToPage("/Index");
    }

    /// <summary>
    /// Выходит из аккаунта по кнопке формы.
    /// </summary>
    /// <returns>Перенаправление на главную страницу.</returns>
    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await signInManager.SignOutAsync();

        return RedirectToPage("/Index");
    }
}