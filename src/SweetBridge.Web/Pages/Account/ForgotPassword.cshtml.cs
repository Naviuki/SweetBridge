using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.ValueObjects;
using SweetBridge.Web.Services.Messaging;

namespace SweetBridge.Web.Pages.Account;

/// <summary>
/// Запрос письма для смены пароля.
/// </summary>
/// <param name="userManager">Менеджер пользователей Identity.</param>
/// <param name="emailComposer">Формировщик писем.</param>
[AllowAnonymous]
public sealed class ForgotPasswordModel(UserManager<AppUser> userManager, IEmailComposer emailComposer) : PageModel
{
    private const string AuthRateLimitPolicy = "Auth";

    /// <summary>
    /// Почта аккаунта.
    /// </summary>
    [BindProperty]
    [Required(ErrorMessage = "Укажите почту.")]
    [EmailAddress(ErrorMessage = "Проверьте адрес электронной почты.")]
    [Display(Name = "Электронная почта")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Отправляет письмо со ссылкой смены пароля.
    /// </summary>
    /// <returns>Результат выполнения.</returns>
    [EnableRateLimiting(AuthRateLimitPolicy)]
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        AppUser? user = await userManager.FindByEmailAsync(Email.Trim());

        if (user is not null)
        {
            string token = await userManager.GeneratePasswordResetTokenAsync(user);
            string code = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(token));
            string link = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { code, email = user.Email },
                protocol: Request.Scheme) ?? "/Account/ResetPassword";

            await emailComposer.SendPasswordResetAsync(user.Email!, link, CancellationToken.None);
        }

        TempData["Info"] = "Если аккаунт с такой почтой существует, письмо со ссылкой для смены пароля уже отправлено.";

        return RedirectToPage();
    }
}