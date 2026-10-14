using SweetBridge.Web.Services.Account;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.ValueObjects;

namespace SweetBridge.Web.Pages.Account;

/// <summary>
/// Профиль покупателя: имя, телефон и смена пароля.
/// </summary>
/// <param name="userManager">Менеджер пользователей Identity.</param>
/// <param name="passwordPolicy">Требования к паролю из настроек Identity.</param>
public sealed class ProfileModel(
    UserManager<AppUser> userManager,
    SweetBridge.Web.Services.Account.PasswordPolicyText passwordPolicy) : PageModel
{
    /// <summary>
    /// <summary>
    /// Данные профиля.
    /// </summary>
    public string PasswordHint => passwordPolicy.Hint;

/// <summary>
    /// Группа полей пароля формы смены: одной кнопкой показываются все три поля.
    /// </summary>
    private const string PasswordGroup = "Password_CurrentPassword Password_NewPassword Password_ConfirmPassword";

    /// <summary>
    /// Поле «Текущий пароль».
    /// </summary>
    public PasswordFieldViewModel CurrentPasswordField { get; } = new(
        Name: "Password.CurrentPassword",
        Id: "Password_CurrentPassword",
        Label: "Текущий пароль",
        Autocomplete: "current-password",
        Group: PasswordGroup);

    /// <summary>
    /// Поле «Новый пароль» с подсказкой из настроек Identity.
    /// </summary>
    public PasswordFieldViewModel NewPasswordField { get; } = new(
        Name: "Password.NewPassword",
        Id: "Password_NewPassword",
        Label: "Новый пароль",
        Autocomplete: "new-password",
        Group: PasswordGroup,
        Hint: passwordPolicy.Hint);

    /// <summary>
    /// Поле подтверждения нового пароля.
    /// </summary>
    public PasswordFieldViewModel ConfirmPasswordField { get; } = new(
        Name: "Password.ConfirmPassword",
        Id: "Password_ConfirmPassword",
        Label: "Повторите новый пароль",
        Autocomplete: "new-password",
        Group: PasswordGroup);

    /// Данные профиля.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Данные смены пароля.
    /// </summary>
    [BindProperty]
    public PasswordModel Password { get; set; } = new();

    /// <summary>
    /// Текущий телефон в нормализованном виде.
    /// </summary>
    public string? CurrentPhone { get; private set; }

    /// <summary>
    /// Загружает профиль.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        AppUser? user = await GetUserAsync();

        if (user is null)
        {
            return RedirectToPage("./Login");
        }

        Input = new InputModel
        {
            Name = user.FullName,
            Email = user.Email ?? string.Empty,
            Phone = user.PhoneNumber ?? string.Empty
        };

        CurrentPhone = user.PhoneNumberNormalized;

        return Page();
    }

    /// <summary>
    /// Сохраняет профиль.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        AppUser? user = await GetUserAsync();

        if (user is null)
        {
            return RedirectToPage("./Login");
        }

        CurrentPhone = user.PhoneNumberNormalized;

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

        user.FullName = Input.Name.Trim();
        user.PhoneNumber = phone;
        user.PhoneNumberNormalized = phone;
        user.PhoneNumberConfirmed = true;

        IdentityResult result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        TempData["Success"] = "Профиль сохранён.";

        return RedirectToPage();
    }

    /// <summary>
    /// Меняет пароль.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task<IActionResult> OnPostChangePasswordAsync(CancellationToken cancellationToken)
    {
        AppUser? user = await GetUserAsync();

        if (user is null)
        {
            return RedirectToPage("./Login");
        }

        CurrentPhone = user.PhoneNumberNormalized;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        IdentityResult result = await userManager.ChangePasswordAsync(user, Password.CurrentPassword, Password.NewPassword);

        if (!result.Succeeded)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError($"{nameof(Password)}.{nameof(Password.NewPassword)}", error.Description);
            }

            return Page();
        }

        TempData["Success"] = "Пароль обновлён.";

        return RedirectToPage();
    }

    private Task<AppUser?> GetUserAsync()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return string.IsNullOrWhiteSpace(userId)
            ? Task.FromResult<AppUser?>(null)
            : userManager.FindByIdAsync(userId);
    }

    /// <summary>
    /// Данные профиля.
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
        /// Электронная почта: только для показа.
        /// </summary>
        [EmailAddress]
        [Display(Name = "Электронная почта")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Телефон.
        /// </summary>
        [Required(ErrorMessage = "Укажите телефон.")]
        [StringLength(40, ErrorMessage = "Телефон не должен быть длиннее 40 символов.")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;
    }

    /// <summary>
    /// Данные смены пароля.
    /// </summary>
    public sealed class PasswordModel
    {
        /// <summary>
        /// Текущий пароль.
        /// </summary>
        [Required(ErrorMessage = "Укажите текущий пароль.")]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// Новый пароль.
        /// </summary>
        [Required(ErrorMessage = "Задайте новый пароль.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль должен быть не короче 8 символов.")]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Подтверждение нового пароля.
        /// </summary>
        [Required(ErrorMessage = "Повторите новый пароль.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Пароли не совпадают.")]
        [Display(Name = "Повторите новый пароль")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
