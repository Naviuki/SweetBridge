using Microsoft.AspNetCore.Mvc.ModelBinding;
using SweetBridge.Web.Services.Account;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SweetBridge.Web.Domain.Entities;

namespace SweetBridge.Web.Pages.Account;

/// <summary>
/// Создание нового пароля по ссылке из письма.
/// </summary>
/// <param name="userManager">Менеджер пользователей Identity.</param>
/// <param name="signInManager">Менеджер входа Identity.</param>
/// <param name="passwordPolicy">Требования к паролю из настроек Identity.</param>
[AllowAnonymous]
public sealed class ResetPasswordModel(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    SweetBridge.Web.Services.Account.PasswordPolicyText passwordPolicy) : PageModel
{
    private const string AuthRateLimitPolicy = "Auth";

    /// <summary>
    /// Требования к паролю из настроек Identity.
    /// </summary>
    [BindNever] // Служебное свойство страницы: не участвует в биндинге модели.
    public string PasswordHint => passwordPolicy.Hint;/// <summary>
    /// Группа полей пароля формы: одной кнопкой показываются оба поля.
    /// </summary>
    private const string PasswordGroup = "Input_Password Input_ConfirmPassword";

    /// <summary>
    /// Поле «Новый пароль» формы сброса.
    /// </summary>
    public PasswordFieldViewModel NewPasswordField { get; } = new(
        Name: "Input.Password",
        Id: "Input_Password",
        Label: "Новый пароль",
        Autocomplete: "new-password",
        Group: PasswordGroup,
        Hint: passwordPolicy.Hint);

    /// <summary>
    /// Поле подтверждения нового пароля.
    /// </summary>
    public PasswordFieldViewModel ConfirmPasswordField { get; } = new(
        Name: "Input.ConfirmPassword",
        Id: "Input_ConfirmPassword",
        Label: "Повторите новый пароль",
        Autocomplete: "new-password",
        Group: PasswordGroup);


    /// <summary>
    /// Данные формы.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Открывает форму по ссылке из письма.
    /// </summary>
    /// <returns>Результат выполнения.</returns>
    public IActionResult OnGet(string? code, string? email) => Page();

    /// <summary>
    /// Сохраняет новый пароль.
    /// </summary>
    /// <returns>Перенаправление на страницу входа.</returns>
    [EnableRateLimiting(AuthRateLimitPolicy)]
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        AppUser? user = await userManager.FindByEmailAsync(Input.Email.Trim());

        if (user is null || string.IsNullOrWhiteSpace(Input.Code))
        {
            return RedirectToPage("./Login");
        }

        string token;
        try
        {
            token = System.Text.Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Input.Code));
        }
        catch (FormatException)
        {
            return RedirectToPage("./Login");
        }

        IdentityResult result = await userManager.ResetPasswordAsync(user, token, Input.Password);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Ссылка для смены пароля недействительна или устарела. Запросите новую.");

            return Page();
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        TempData["Success"] = "Пароль обновлён. Теперь войдите с новым паролем.";

        return RedirectToPage("./Login");
    }

    /// <summary>
    /// Форма нового пароля.
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
        /// Токен сброса из ссылки.
        /// </summary>
        [Required]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Новый пароль.
        /// </summary>
        [Required(ErrorMessage = "Задайте новый пароль.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль должен быть не короче 8 символов.")]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Подтверждение пароля.
        /// </summary>
        [Required(ErrorMessage = "Повторите новый пароль.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают.")]
        [Display(Name = "Повторите новый пароль")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
