using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Forms;

namespace SweetBridge.Web.Pages.Support;

/// <summary>
/// Форма обращения в поддержку.
/// </summary>
/// <param name="formsService">Сервис форм.</param>
public sealed class IndexModel(IFormsService formsService) : PageModel
{
    private const string FormsRateLimitPolicy = "Forms";

    /// <summary>
    /// Данные обращения.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Отправляет обращение.
    /// </summary>
    /// <returns>Результат выполнения.</returns>
    [EnableRateLimiting(FormsRateLimitPolicy)]
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await formsService.CreateSupportRequestAsync(Input.Name, Input.Email, Input.OrderNumber, Input.Message, CancellationToken.None);
            TempData["Success"] = "Обращение отправлено. Ответим на указанную почту в течение рабочего дня.";
        }
        catch (DomainException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
        }

        return RedirectToPage();
    }

    /// <summary>
    /// Данные формы обращения.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Имя.
        /// </summary>
        [Required(ErrorMessage = "Укажите имя.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Имя должно быть от 2 до 200 символов.")]
        [Display(Name = "Имя")]
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
        /// Номер заказа.
        /// </summary>
        [StringLength(16, ErrorMessage = "Номер заказа выглядит как SW-XXXXXX.")]
        [Display(Name = "Номер заказа")]
        public string? OrderNumber { get; set; }

        /// <summary>
        /// Текст обращения.
        /// </summary>
        [Required(ErrorMessage = "Напишите текст обращения.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Текст обращения должен быть от 10 до 2000 символов.")]
        [Display(Name = "Сообщение")]
        public string Message { get; set; } = string.Empty;
    }
}