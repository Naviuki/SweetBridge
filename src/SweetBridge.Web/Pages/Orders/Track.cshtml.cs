using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using SweetBridge.Web.Services.Orders;

namespace SweetBridge.Web.Pages.Orders;

/// <summary>
/// Отслеживание заказа по номеру и телефону.
/// </summary>
/// <param name="orderService">Сервис заказов.</param>
public sealed class TrackModel(IOrderService orderService) : PageModel
{
    private const string TrackingRateLimitPolicy = "Tracking";

    /// <summary>
    /// Номер заказа из формы.
    /// </summary>
    [BindProperty]
    public string? Number { get; set; }

    /// <summary>
    /// Телефон покупателя или получателя.
    /// </summary>
    [BindProperty]
    public string? Phone { get; set; }

    /// <summary>
    /// Найденный заказ.
    /// </summary>
    public OrderSummary? Order { get; private set; }

    /// <summary>
    /// Признак того, что запрос выполнен, но заказ не найден.
    /// </summary>
    public bool IsSearched { get; private set; }

    /// <summary>
    /// Открывает форму поиска.
    /// </summary>
    /// <returns>Результат выполнения.</returns>
    public IActionResult OnGet() => Page();

    /// <summary>
    /// Ищет заказ по номеру и телефону.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    [EnableRateLimiting(TrackingRateLimitPolicy)]
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        IsSearched = true;

        if (string.IsNullOrWhiteSpace(Number) || string.IsNullOrWhiteSpace(Phone))
        {
            ModelState.AddModelError(string.Empty, "Введите номер заказа и телефон, указанный при оформлении.");

            return Page();
        }

        try
        {
            Order = await orderService.FindAsync(Number, Phone, cancellationToken);
        }
        catch (Domain.Exceptions.DomainException)
        {
            ModelState.AddModelError(string.Empty, "Проверьте номер заказа и телефон: номер выглядит как SW-XXXXXX.");

            return Page();
        }

        if (Order is null)
        {
            TempData["Info"] = "Заказ с таким номером и телефоном не найден. Проверьте данные или напишите в поддержку.";
        }

        return Page();
    }
}