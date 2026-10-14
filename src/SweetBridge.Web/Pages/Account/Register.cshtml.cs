using Microsoft.AspNetCore.Mvc.ModelBinding;
using SweetBridge.Web.Services.Account;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.ValueObjects;
using SweetBridge.Web.Services.Common;

namespace SweetBridge.Web.Pages.Account;

/// <summary>
/// Регистрация покупателя.
/// </summary>
/// <param name="userManager">Менеджер пользователей Identity.</param>
/// <param name="signInManager">Менеджер входа Identity.</param>
/// <param name="clock">Источник времени.</param>
/// <param name="passwordPolicy">Требования к паролю из настроек Identity.</param>
/// <param name="passwordPolicy">Текст требований к паролю из настроек Identity.</param>
[AllowAnonymous]
public sealed class RegisterModel(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IBusinessClock clock,
    SweetBridge.Web.Services.Account.PasswordPolicyText passwordPolicy) : PageModel
{
    private const string AuthRateLimitPolicy = "Auth";

    /// <summary>
    /// Данные формы регистрации.
    /// </summary>
    [BindProperty]
    /// <summary>
    /// Требования к паролю из настроек Identity: подсказка всегда совпадает с проверками сервера.
    /// </summary>
    [BindNever] // Служебное свойство страницы: не участвует в биндинге модели.
    public string PasswordHint => passwordPolicy.Hint;/// <summary>
    /// Группа полей пароля формы: одной кнопкой показываются оба поля.
    /// </summary>
    private const string PasswordGroup = "Input_Password Input_ConfirmPassword";

    /// <summary>
    /// Поле «Пароль» формы регистрации.
    /// </summary>
    public PasswordFieldViewModel NewPasswordField { get; } = new(
        Name: "Input.Password",
        Id: "Input_Password",
        Label: "Пароль",
        Autocomplete: "new-password",
        Group: PasswordGroup,
        Hint: passwordPolicy.Hint);

    /// <summary>
    /// Поле «Повторите пароль» формы регистрации.
    /// </summary>
    public PasswordFieldViewModel ConfirmPasswordField { get; } = new(
        Name: "Input.ConfirmPassword",
        Id: "Input_ConfirmPassword",
        Label: "Повторите пароль",
        Autocomplete: "new-password",
        Group: PasswordGroup);

    /// <summary>
    /// Данные формы регистрации.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Регистрирует покупателя.
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

        string? phone = PhoneNumber.TryNormalize(Input.Phone);

        if (phone is null)
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Phone)}", "Укажите телефон в формате +7 999 000-11-22.");

            return Page();
        }

        var user = new AppUser
        {
            UserName = Input.Email.Trim(),
            Email = Input.Email.Trim(),
            FullName = Input.Name.Trim(),
            PhoneNumber = phone,
            PhoneNumberConfirmed = true,
            PhoneNumberNormalized = phone,
            EmailConfirmed = true,
            CreatedAtUtc = clock.UtcNow
        };

        IdentityResult result = await userManager.CreateAsync(user, Input.Password);

        if (!result.Succeeded)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, Describe(error));
            }

            return Page();
        }

        await signInManager.SignInAsync(user, isPersistent: true);
        TempData["Success"] = "Аккаунт создан. Добро пожаловать в «Сладкий мост»!";

        return RedirectToPage("./Orders");
    }

    private static string Describe(IdentityError error) => error.Code switch
    {
        "DuplicateUserName" or "DuplicateEmail" => "Аккаунт с такой почтой уже существует.",
        "PasswordTooShort" => "Пароль должен быть не короче 8 символов.",
        "PasswordRequiresDigit" => "Пароль должен содержать цифру.",
        "PasswordRequiresUpper" => "Пароль должен содержать заглавную букву.",
        "PasswordRequiresNonAlphanumeric" => "Пароль должен содержать специальный символ.",
        _ => error.Description
    };

    /// <summary>
    /// Форма регистрации.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Имя и фамилия.
        /// </summary>
        [Required(ErrorMessage = "Укажите имя.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Имя должно быть от 2 до 200 символов.")]
        [Display(Name = "Имя и фамилия")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Электронная почта.
        /// </summary>
        [Required(ErrorMessage = "Укажите почту.")]
        [EmailAddress(ErrorMessage = "Проверьте адрес электронной почты.")]
        [StringLength(320, ErrorMessage = "Почта не должна быть длиннее 320 символов.")]
        [Display(Name = "Электронная почта")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Телефон.
        /// </summary>
        [Required(ErrorMessage = "Укажите телефон для доставки.")]
        [StringLength(40, ErrorMessage = "Телефон не должен быть длиннее 40 символов.")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Пароль.
        /// </summary>
        [Required(ErrorMessage = "Задайте пароль.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль должен быть не короче 8 символов.")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Подтверждение пароля.
        /// </summary>
        [Required(ErrorMessage = "Повторите пароль.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают.")]
        [Display(Name = "Повторите пароль")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
