using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Forms;

namespace SweetBridge.Web.Pages.Corporate;

/// <summary>
/// Заявка на корпоративную поставку.
/// </summary>
/// <param name="formsService">Сервис форм.</param>
public sealed class IndexModel(IFormsService formsService) : PageModel
{
    private const string FormsRateLimitPolicy = "Forms";

    /// <summary>
    /// Данные заявки.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Отправляет заявку.
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
            await formsService.CreateCorporateRequestAsync(
                Input.Company,
                Input.ContactName,
                Input.Email,
                Input.Volume,
                Input.Direction,
                Input.Comment,
                CancellationToken.None);

            TempData["Success"] = "Заявка отправлена. Свяжемся с вами по почте в течение рабочего дня.";
        }
        catch (DomainException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
        }

        return RedirectToPage();
    }

    /// <summary>
    /// Данные формы заявки.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Компания или организация.
        /// </summary>
        [Required(ErrorMessage = "Укажите компанию.")]
        [StringLength(200, ErrorMessage = "Название компании не должно быть длиннее 200 символов.")]
        [Display(Name = "Компания")]
        public string Company { get; set; } = string.Empty;

        /// <summary>
        /// Контактное лицо.
        /// </summary>
        [Required(ErrorMessage = "Укажите контактное лицо.")]
        [StringLength(200, ErrorMessage = "Имя не должно быть длиннее 200 символов.")]
        [Display(Name = "Контактное лицо")]
        public string ContactName { get; set; } = string.Empty;

        /// <summary>
        /// Электронная почта.
        /// </summary>
        [Required(ErrorMessage = "Укажите почту.")]
        [EmailAddress(ErrorMessage = "Проверьте адрес электронной почты.")]
        [StringLength(320, ErrorMessage = "Почта не должна быть длиннее 320 символов.")]
        [Display(Name = "Электронная почта")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Ориентировочный объём.
        /// </summary>
        [Required(ErrorMessage = "Укажите объём заказа.")]
        [StringLength(100, ErrorMessage = "Описание объёма не должно быть длиннее 100 символов.")]
        [Display(Name = "Объём заказа")]
        public string Volume { get; set; } = string.Empty;

        /// <summary>
        /// Направление поставки.
        /// </summary>
        [Display(Name = "Направление")]
        public ShippingDirection Direction { get; set; } = ShippingDirection.YakutiaToDonbass;

        /// <summary>
        /// Комментарий.
        /// </summary>
        [StringLength(2000, ErrorMessage = "Комментарий не должен быть длиннее 2000 символов.")]
        [Display(Name = "Комментарий")]
        public string? Comment { get; set; }
    }
}