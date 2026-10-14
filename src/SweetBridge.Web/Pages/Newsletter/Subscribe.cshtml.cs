using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Forms;
using SweetBridge.Web.Services.Messaging;

namespace SweetBridge.Web.Pages.Newsletter;

/// <summary>
/// Подписка на рассылку о новых волнах.
/// </summary>
/// <param name="formsService">Сервис форм.</param>
/// <param name="emailComposer">Формировщик писем.</param>
public sealed class SubscribeModel(IFormsService formsService, IEmailComposer emailComposer) : PageModel
{
    private const string FormsRateLimitPolicy = "Forms";

    /// <summary>
    /// Адрес для подписки.
    /// </summary>
    [BindProperty]
    [Required(ErrorMessage = "Укажите адрес электронной почты.")]
    [EmailAddress(ErrorMessage = "Проверьте адрес электронной почты.")]
    [StringLength(320, ErrorMessage = "Почта не должна быть длиннее 320 символов.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Подписывает адрес и подтверждает подписку по ссылке из письма.
    /// </summary>
    /// <param name="slug">Идентификатор действия: subscribe или confirm.</param>
    /// <param name="token">Токен подтверждения.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task<IActionResult> OnGetAsync(string? slug, string? token)
    {
        if (string.Equals(slug, "confirm", StringComparison.OrdinalIgnoreCase))
        {
            bool confirmed = await formsService.ConfirmSubscriptionAsync(token, CancellationToken.None);
            TempData[confirmed ? "Success" : "Error"] = confirmed
                ? "Подписка подтверждена. До встречи на новых волнах!"
                : "Ссылка подтверждения недействительна или устарела.";

            return RedirectToPage("/Index");
        }

        return new EmptyResult();
    }

    /// <summary>
    /// Принимает адрес для подписки.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    [EnableRateLimiting(FormsRateLimitPolicy)]
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return new EmptyResult();
        }

        try
        {
            string? token = await formsService.SubscribeAsync(Email, cancellationToken);

            if (token is null)
            {
                TempData["Info"] = "Этот адрес уже подписан на рассылку.";
            }
            else
            {
                string link = Url.Page("/Newsletter/Subscribe", null, new { slug = "confirm", token }, Request.Scheme)
                    ?? "/Newsletter/Subscribe";

                await emailComposer.SendSubscriptionConfirmationAsync(Email.Trim(), link, cancellationToken);
                TempData["Success"] = "Почти готово: мы отправили письмо для подтверждения подписки.";
            }
        }
        catch (DomainException exception)
        {
            TempData["Error"] = exception.Message;
        }

        string? referer = Request.Headers.Referer.ToString();

        return Url.IsLocalUrl(referer) ? LocalRedirect(referer) : RedirectToPage("/Index");
    }
}