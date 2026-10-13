using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using SweetWave.Web.Options;

namespace SweetWave.Web.Pages.Admin;

/// <summary>
/// Страница входа администратора. Учётные данные берутся из конфигурации и сравниваются
/// за постоянное время.
/// </summary>
/// <param name="options">Учётные данные администратора.</param>
[AllowAnonymous]
public sealed class LoginModel(IOptions<AdminOptions> options) : PageModel
{
    private readonly AdminOptions _adminOptions = options.Value;

    /// <summary>
    /// Данные формы входа.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Адрес перенаправления после успешного входа.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Данные формы входа.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Логин администратора.
        /// </summary>
        [Required(ErrorMessage = "Укажите логин.")]
        [StringLength(100, ErrorMessage = "Логин слишком длинный.")]
        [Display(Name = "Логин")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Пароль администратора.
        /// </summary>
        [Required(ErrorMessage = "Укажите пароль.")]
        [StringLength(200, ErrorMessage = "Пароль слишком длинный.")]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Проверяет корректность входных данных.
    /// </summary>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    public void OnGet()
    {
    }

    /// <summary>
    /// Проверяет учётные данные и выдаёт cookie аутентификации.
    /// </summary>
    /// <returns>Перенаправление на страницу товаров админки или обратно на форму.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!CredentialsMatch(Input.UserName, _adminOptions.UserName)
            || !CredentialsMatch(Input.Password, _adminOptions.Password))
        {
            ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
            return Page();
        }

        ClaimsIdentity identity = new(
            [new Claim(ClaimTypes.Name, _adminOptions.UserName)],
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return LocalRedirectOrHome(ReturnUrl);
    }

    private static bool CredentialsMatch(string entered, string expected) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(entered ?? string.Empty),
            Encoding.UTF8.GetBytes(expected ?? string.Empty));

    private IActionResult LocalRedirectOrHome(string? url) =>
        !string.IsNullOrEmpty(url) && Url.IsLocalUrl(url)
            ? Redirect(url)
            : RedirectToPage("/Admin/Products/Index");
}
