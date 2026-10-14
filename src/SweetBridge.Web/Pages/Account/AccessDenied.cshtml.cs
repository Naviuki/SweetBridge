using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SweetBridge.Web.Pages.Account;

/// <summary>
/// Страница отказа в доступе.
/// </summary>
[AllowAnonymous]
public sealed class AccessDeniedModel : PageModel
{
    /// <summary>
    /// Открывает страницу.
    /// </summary>
    /// <returns>Результат выполнения.</returns>
    public IActionResult OnGet() => Page();
}