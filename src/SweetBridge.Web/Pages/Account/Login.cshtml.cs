using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using SweetBridge.Web.Options;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Common;
using SweetBridge.Web.Services.Messaging;

namespace SweetBridge.Web.Pages.Account;

/// <summary>
/// Вход в личный кабинет.
/// </summary>
/// <param name="signInManager">Менеджер входа Identity.</param>
/// <param name="externalAuth">Параметры внешней авторизации.</param>
[AllowAnonymous]
public sealed class LoginModel(
    SignInManager<AppUser> signInManager,
    IOptions<ExternalAuthOptions> externalAuth) : PageModel
{
    private const string AuthRateLimitPolicy = "Auth";

    private IOptions<ExternalAuthOptions> ExternalAuth => externalAuth;

    /// <summary>
    /// Данные формы входа.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Адрес возврата.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Показывает форму входа.
    /// </summary>
    /// <returns>Результат выполнения.</returns>
    public IActionResult OnGet() => Page();

    /// <summary>
    /// Выполняет вход.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление в личный кабинет.</returns>
    [EnableRateLimiting(AuthRateLimitPolicy)]
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cancellationToken);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(
            Input.Email,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                result.IsLockedOut
                    ? "Аккаунт временно заблокирован из-за частых попыток входа. Попробуйте позже."
                    : "Неверная почта или пароль.");

            return Page();
        }

        if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return RedirectToPage("./Orders");
    }

    /// <summary>
    /// Форма входа.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Электронная почта.
        /// </summary>
        [Required(ErrorMessage = "Укажите почту.")]
        [EmailAddress(ErrorMessage = "Проверьте адрес электронной почты.")]
        [Display(Name = "Электронная почта")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Пароль.
        /// </summary>
        [Required(ErrorMessage = "Укажите пароль.")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Запомнить меня.
        /// </summary>
        [Display(Name = "Не выходить из аккаунта")]
        public bool RememberMe { get; set; }
    }
}